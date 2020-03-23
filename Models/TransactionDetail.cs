using System;

namespace zoneswitch.metricsgenerator.Models
{
    public class FTTransactionDetail
    {
        public string SenderBank { get; set; }
        public string TransactionType { get; set; }
        public DateTime StartTime { get; set; }
        public double Amount { get; set; }
    }
}