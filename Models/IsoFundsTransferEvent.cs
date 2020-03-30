namespace zoneswitch.metricsgenerator.Models
{
    public class IsoFundsTransferEvent
    {
        public string DateUpdated { get; set; }
        public string TransactionType { get; set; }
        public string MessageTypeIndicator { get; set; }
        public string MsgTypeAndTransactionReference { get; set; }
        public string ResponseCode { get; set; }
        public string FromSwitch { get; set; }
    }
}