using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using zoneswitch.metricsgenerator.Repository;
using zoneswitch.metricsgenerator.Extensions;
using zoneswitch.metricsgenerator.Models.Events;

namespace zoneswitch.metricsgenerator
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private string GroupName = "ZoneSwitch";
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Creates a subscribtion to ZSTransaction events (Both Card and Accounts)
            Start();
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(60000, stoppingToken);
            }
        }
        
        public void Start()
        {
            var appsettings = new AppSettings();
            using (StreamReader r = File.OpenText("appsettings.json"))
            {
                string json = r.ReadToEnd();
                json = json.Replace("\r\n","").Trim();
                appsettings = JsonConvert.DeserializeObject<AppSettings>(json);
            }
            try
            {
                PersistentSubscriptionSettings settings = PersistentSubscriptionSettings.Create()
                    .DoNotResolveLinkTos()
                    .StartFromCurrent();
                
                var user = appsettings.EventStoreUser;
                var password = appsettings.EventStorePassword;
                var connection = EventStoreHost.ConnectToEventStore();
                connection.CreatePersistentSubscriptionAsync(FundsTransferEvents.STREAM_NAME, GroupName, settings, new UserCredentials(user, password));
                connection.ConnectToPersistentSubscription(FundsTransferEvents.STREAM_NAME, GroupName, EventSubscriber.Process, null, new UserCredentials(user, password));
            }
            catch (Exception) 
            {
            }
            // return true;
        }



        
    }
}
