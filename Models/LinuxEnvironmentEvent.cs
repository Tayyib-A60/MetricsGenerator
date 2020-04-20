using System.Collections.Generic;

namespace zoneswitch.metricsgenerator.Models
{
    public class LinuxEnvironmentEvent {
        public List<LinuxServiceMetrics> ServiceStatistics { get; set; }
        public List<LinuxServerMetrics> SystemStatistics { get; set; }
    }
}