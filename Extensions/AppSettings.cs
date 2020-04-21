namespace zoneswitch.metricsgenerator.Extensions
{
    public class AppSettings
    {
        public string EventStoreServerIp { get; set; }
        public string EventStorePort { get; set; }
        public string EventStoreUser { get; set; }
        public string EventStorePassword { get; set; }
        public string InfluxDbUser { get; set; }
        public string InfluxDbPassword { get; set; }
        public string InfluxDbUrl { get; set; }
        public string BankCode { get; set; }
        public string SqlServerConnectionString { get; set; }
        public string UniqueAccountTable { get; set; }
        public string UniqueCardTable { get; set; }
        public string UniqueMetricsInterval { get; set; }
    }
}