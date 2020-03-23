namespace zoneswitch.metricsgenerator.Models
{
    public class TransactionCompletedEvent
    {
        public string transactionReference { get; set; }
        public string status { get; set; }
        public string responseMessage { get; set; }
        public string step1status { get; set; }
        public string step2status { get; set; }
        public string step3status { get; set; }
        public string step4status { get; set; }
        public string dateUpdated { get; set; }
        public string transactionId { get; set; }
    }
}