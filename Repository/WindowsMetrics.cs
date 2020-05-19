using System;
using InfluxDB.Client.Core;

[Measurement("windowsMetrics")]
public class WindowsMetrics
{
    [Column("totalDiskSpace", IsTag = true)] public long TotalDiskSpace { get; set; }

    [Column("availableDiskSpace")] public long AvailableDiskSpace { get; set; }
    [Column("processor")] public float Processor { get; set; }
    [Column("availableRamInMB")] public float AvailableRamInMB { get; set; }

    [Column(IsTimestamp = true)] public DateTime Time;
}