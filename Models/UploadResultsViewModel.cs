using System.Collections.Generic;

namespace CallPatternAnalyzer.Models
{
    public class UploadResultsViewModel
    {
        public List<CallRecord> Records { get; set; } = new();

        public int TotalRecords { get; set; }

        public double AverageCallMinutes { get; set; }

        public Dictionary<string, int> CallsByTimeOfDay { get; set; } = new();

        public Dictionary<string, int> CallsByStatus { get; set; } = new();

        public Dictionary<string, int> TopPeopleByCallCount { get; set; } = new();

        public List<CallClusterSummary> ClusterSummaries { get; set; } = new();

        public Dictionary<string, double> AverageMinutesByTimeOfDay { get; set; } = new();

        public Dictionary<string, double> AverageMinutesByStatus { get; set; } = new();
    }
}