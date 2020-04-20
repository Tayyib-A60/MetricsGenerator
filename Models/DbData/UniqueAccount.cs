using System;

namespace zoneswitch.metricsgenerator.Models.DbData
{
    public class UniqueAccount
    {
        public long Id { get; set; }
        public string AccountNumber { get; set; }
        public string TransactionType { get; set; }
        public int TransactionCount { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string MonthYear { get; set; }
        public string StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string BillingWindow { get; set; }
        public string BilledStatus { get; set; }
    }
}