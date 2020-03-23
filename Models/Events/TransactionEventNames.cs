namespace zoneswitch.metricsgenerator.Models.Events
{
    public class FundsTransferEvents
    {
        // Stream name
        public const string STREAM_NAME = "ZSTransactions";
        // Event names
        public const string INITIATED = "FundsTransferInitiated";
        public const string FAILED = "FundsTransferFailed";
        public const string PROCESSED = "FundsTransferProcessed";
        public const string COMPLETED = "FundsTransferCompleted";
        public const string DELETED = "FundsTransferDeleted";
    }
}