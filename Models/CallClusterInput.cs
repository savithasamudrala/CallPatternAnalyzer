using Microsoft.ML.Data;

namespace CallPatternAnalyzer.Models;

public class CallClusterInput
{
    public string Name { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public float CallMinutes { get; set; }
    public float HourOfDay { get; set; }
}