namespace zoneswitch.metricsgenerator.Models
{
    public class NameInquiryProcessedEvent
    {
        public string TransactionReference { get; set; }
        public string ReceiverBank { get; set; }
        public string ReceiverAccount { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage{ get; set; }
        public string Status { get; set; }
        public string SenderBank { get; set; }
        public string DateUpdated { get; set; }
    }
}