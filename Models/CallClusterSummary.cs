namespace CallPatternAnalyzer.Models;

public class CallClusterSummary
{
    public uint ClusterId { get; set; }
    public int RecordCount { get; set; }
    public double AverageCallMinutes { get; set; }
    public double AverageHourOfDay { get; set; }
    public string MostCommonStatus { get; set; } = string.Empty;
    public string MostCommonTimeOfDay { get; set; } = string.Empty;
}