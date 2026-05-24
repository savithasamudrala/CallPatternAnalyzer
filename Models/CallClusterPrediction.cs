using Microsoft.ML.Data;
using System;

namespace CallPatternAnalyzer.Models;

public class CallClusterPrediction
{
    [ColumnName("PredictedLabel")]
    public uint ClusterId { get; set; }

    public float[] Score { get; set; } = Array.Empty<float>();
}