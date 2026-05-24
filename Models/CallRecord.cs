using System;

namespace CallPatternAnalyzer.Models;

public class CallRecord
{
    public string Name { get; set; } = string.Empty;

    public string StatusName { get; set; } = string.Empty;

    public float? CallMinutes { get; set; }

    public DateTime? CreatedOn { get; set; }

    public float HourOfDay => CreatedOn?.Hour ?? 0;

    public string TimeOfDayBucket
    {
        get
        {
            if (CreatedOn == null)
            {
                return "Unknown";
            }

            var hour = CreatedOn.Value.Hour;

            if (hour >= 5 && hour < 12)
            {
                return "Morning";
            }

            if (hour >= 12 && hour < 17)
            {
                return "Afternoon";
            }

            if (hour >= 17 && hour < 21)
            {
                return "Evening";
            }

            return "Night";
        }
    }
}