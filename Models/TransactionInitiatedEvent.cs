namespace zoneswitch.metricsgenerator.Models
{
    public class TransactionInitiatedEvent
    {
        public string tmId{ get; set; }   
        public string transactionReference { get; set; }
        public string receiverBankId { get; set; }
        public string senderBankId { get; set; }
        public double amount { get; set; }
        public string currencyName { get; set; }
        public string userTerminalId { get; set; }
        public string senderAccount { get; set; }
        public string receiverAccountNo { get; set; }
        public string currencyCode { get; set; }
        public string transactionType { get; set; }
        public string nameInquiryReference { get; set; }
        public string status { get; set; }
        public string Step1Status { get; set; }
        public string Step2Status { get; set; }
        public string dateCreated { get; set; }
        public string dateUpdated { get; set; }

    }
}