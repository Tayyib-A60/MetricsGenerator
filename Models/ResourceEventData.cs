namespace zoneswitch.metricsgenerator.Models
{
    public class ResourceEventData
    {
        public long TotalDiskSpace { get; set; }
        public long AvailableDiskSpace { get; set; }
        public float Processor { get; set; }
        public float AvailableRamInMB { get; set; }
    }
}