namespace zoneswitch.metricsgenerator.Models.Events
{
    public class NameInquiryEvents
    {
        public const string STREAM_NAME = "ZSNameInquiry";

        public const string INITIATED = "NameInquiryInitiated";
        public const string FAILED = "NameInquiryFailed";
        public const string PENDING = "NameInquiryPending";
        public const string PROCESSED = "NameInquiryProcessed";
        public const string COMPLETED = "NameInquiryCompleted";
    }
}