using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using zoneswitch.metricsgenerator.Extensions;
using zoneswitch.metricsgenerator.Models.Events;
using zoneswitch.metricsgenerator.Repository;

namespace zoneswitch.metricsgenerator {
    public class Worker : BackgroundService {
        private readonly ILogger<Worker> _logger;
        public AppSettings appSettings { get; set; }
        private string FTGroupName = "ZoneSwitchFT";
        private string NIGroupName = "ZoneSwitchNI";
        private string ResourceGroupName = "ZoneSwitchResources";
        public static bool _islocked;
        private UniqueAccountProcessor _accountProcessor { get; }
        private UniqueCardProcessor _cardProcessor { get; }
        public Worker (ILogger<Worker> logger, 
                        UniqueAccountProcessor accountProcessor,
                        UniqueCardProcessor cardProcessor) 
        {
            _cardProcessor = cardProcessor;
            _accountProcessor = accountProcessor;
            _logger = logger;
        }
        protected override async Task ExecuteAsync (CancellationToken stoppingToken) {
            // Creates a subscribtion to ZSTransaction events (Both Card and Accounts)
            Start ();
            RunUniqueProcessors();
            while (!stoppingToken.IsCancellationRequested) {
                _logger.LogInformation ("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay (60000, stoppingToken);
            }
        }

        public void Start () {
            var appsettings = new AppSettings ();
            using (StreamReader r = File.OpenText ("appsettings.json")) {
                string json = r.ReadToEnd ();
                json = json.Replace ("\r\n", "").Trim ();
                appsettings = JsonConvert.DeserializeObject<AppSettings> (json);
            }
            try {
                PersistentSubscriptionSettings settings = PersistentSubscriptionSettings.Create ()
                    .DoNotResolveLinkTos ()
                    .StartFromCurrent ();

                var user = appsettings.EventStoreUser;
                var password = appsettings.EventStorePassword;

                var connection = EventStoreHost.ConnectToEventStore ();
                // Connect to FT Stream
                connection.CreatePersistentSubscriptionAsync (FundsTransferEvents.STREAM_NAME, FTGroupName, settings, new UserCredentials (user, password));
                connection.ConnectToPersistentSubscription (FundsTransferEvents.STREAM_NAME, FTGroupName, EventSubscriber.Process, null, new UserCredentials (user, password));

                // Connect to NI Stream
                connection.CreatePersistentSubscriptionAsync (NameInquiryEvents.STREAM_NAME, NIGroupName, settings, new UserCredentials (user, password));
                connection.ConnectToPersistentSubscription (NameInquiryEvents.STREAM_NAME, NIGroupName, EventSubscriber.Process, null, new UserCredentials (user, password));

                // Connect to WindowResource Stream
                connection.CreatePersistentSubscriptionAsync (EnvironmentSpecificEvents.WINDOWS_STREAM_NAME, ResourceGroupName, settings, new UserCredentials (user, password));
                connection.ConnectToPersistentSubscription (EnvironmentSpecificEvents.WINDOWS_STREAM_NAME, ResourceGroupName, EventSubscriber.Process, null, new UserCredentials (user, password));

                // Connect to LinuxEnvironment Stream
                connection.CreatePersistentSubscriptionAsync (EnvironmentSpecificEvents.LINUX_STREAM_NAME, ResourceGroupName, settings, new UserCredentials (user, password));
                connection.ConnectToPersistentSubscription (EnvironmentSpecificEvents.LINUX_STREAM_NAME, ResourceGroupName, EventSubscriber.Process, null, new UserCredentials (user, password));
            } catch (Exception) { }
            // return true;
        }

        private void RunUniqueProcessors () {
            appSettings = new AppSettings ();
            using (StreamReader r = File.OpenText ("appsettings.json")) {
                string json = r.ReadToEnd ();
                json = json.Replace ("\r\n", "").Trim ();
                appSettings = new AppSettings ();
                appSettings = JsonConvert.DeserializeObject<AppSettings> (json);
            }

            _logger.LogDebug ("Fetching new unique cards and accounts");

            var canConvertBDInterval = Double.TryParse (appSettings.UniqueMetricsInterval, out double bdInterval);

            var _timer = new System.Timers.Timer (bdInterval);
            _timer.Elapsed += new ElapsedEventHandler (CallProcessor);
            _timer.Start ();
        }

        private async void CallProcessor (Object stateInfo, ElapsedEventArgs e) {
            if (_islocked) {
                _logger.LogDebug ("Still processing some unique metrics");
                return;
            }
            _islocked = true;

            var cardsFromDb = await _cardProcessor.GetUniqueCards();

            if(cardsFromDb.Count > 0) {
                var cardsPostedIds = await MetricsProcessor.ProcessUniqueCards(cardsFromDb);
                var updatedCards = await _cardProcessor.UpdateUniqueCards(cardsPostedIds);

                if(updatedCards) {
                    _logger.LogDebug("Cards updated to being monitored for the current month");
                }
            }

            var accountsFromDb = await _accountProcessor.GetUniqueAccounts();

            if(accountsFromDb.Count > 0) {
                var accountPostedIds = await MetricsProcessor.ProcessUniqueAccounts(accountsFromDb);
                var updatedAccounts = await _accountProcessor.UpdateUniqueAccounts(accountPostedIds);

                if(updatedAccounts) {
                    _logger.LogDebug("Accounts updated to being monitored for the current month");
                }
            }
            _islocked = false;
        }

    }
}