using System;
using System.IO;
using System.Net;
using EventStore.ClientAPI;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using zoneswitch.metricsgenerator.Extensions;

namespace zoneswitch.metricsgenerator.Repository
{
    public class EventStoreHost
    {
        public static AppSettings appsettings { get; set; }
        private static bool _connected;
        public static readonly string EventStoreLogFileName = "./eventstore-log.log";
        public EventStoreHost()
        {
            if (!_connected)
            {
                ConnectToEventStore();
            }
        }
        public static IEventStoreConnection ConnectToEventStore()
        {
            appsettings = new AppSettings();
            using (StreamReader r = File.OpenText("appsettings.json"))
            {
                string json = r.ReadToEnd();
                json = json.Replace("\r\n","").Trim();
                appsettings = new AppSettings();
                appsettings = JsonConvert.DeserializeObject<AppSettings>(json);
            }

            var canConvert = Int32.TryParse(appsettings.EventStorePort, out int port);
            var ipAddress = IPAddress.Parse(appsettings.EventStoreServerIp);
            if(!canConvert)
                Console.WriteLine("Invalid port conversion for event store port in appsettings");
            var settings = ConnectionSettings.Create();
            var EventStoreConn = EventStoreConnection.Create(settings, new IPEndPoint(ipAddress, port));

            EventStoreConn.Closed += _EventStoreConn_Closed;

            EventStoreConn.ConnectAsync().Wait();
            return EventStoreConn;
        }

        private static void _EventStoreConn_Closed(object sender, ClientClosedEventArgs e)
        {
            _connected = false;
        }
    }
}