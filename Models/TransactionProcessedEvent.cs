namespace zoneswitch.metricsgenerator.Models
{
    public class TransactionProcessedEvent
    {
        public string transactionReference { get; set; }
        public string issuerCBATransactionId { get; set; }
        public string dateUpdated { get; set; }
        public string status { get; set; }
        public string Reason { get; set; }
        public string ResponseCode { get; set; }
        public string ReversalStatus { get; set; }
    }
}