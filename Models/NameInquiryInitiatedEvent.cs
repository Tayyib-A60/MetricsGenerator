namespace zoneswitch.metricsgenerator.Models
{
    public class NameInquiryInitiatedEvent
    {
        public string Status { get; set; }
        public string TransactionReference { get; set; }
        public string ReceiverBank { get; set; }
        public string ReceiverAccount { get; set; }
        // public bool IsNuban { get; set; }
        public string SenderBank { get; set; }
        public string DateUpdated { get; set; }
    }
}