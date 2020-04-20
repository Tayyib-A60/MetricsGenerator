namespace zoneswitch.metricsgenerator.Models
{
    public class LinuxServiceMetrics
    {
        public long AverageResponseTime { get; set; }
        public string ServiceName { get; set; }
        public long RequestRate { get; set; }
        public long ResponseRate { get; set; }
        public int SuccessRate { get; set; }
        public string FunctionName { get; set; }
    }
}