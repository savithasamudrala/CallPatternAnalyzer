using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CallPatternAnalyzer.Models;

public class AnalysisResultDocument
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string FileName { get; set; } = string.Empty;

    public int TotalRecords { get; set; }

    public double AverageCallMinutes { get; set; }

    public Dictionary<string, int> CallsByTimeOfDay { get; set; } = new();

    public Dictionary<string, int> CallsByStatus { get; set; } = new();

    public Dictionary<string, double> AverageMinutesByTimeOfDay { get; set; } = new();

    public Dictionary<string, double> AverageMinutesByStatus { get; set; } = new();

    public List<CallClusterSummary> ClusterSummaries { get; set; } = new();

    public List<string> KeyInsights { get; set; } = new();
}