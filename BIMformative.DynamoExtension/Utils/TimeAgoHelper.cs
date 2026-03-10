using System;

namespace BIMformative.DynamoExtension.Utils
{
    public static class TimeAgoHelper
    {
        public static string Format(DateTime dateTime)
        {
            var now = DateTime.Now;
            var ts = now - dateTime;

            if (ts.TotalSeconds < 60)
                return "Updated just now";

            if (ts.TotalMinutes < 60)
                return $"Updated {(int)ts.TotalMinutes} minutes(s) ago";

            if (ts.TotalHours < 24)
                return $"Updated {(int)ts.TotalHours} hour(s) ago";

            if (ts.TotalDays < 7)
                return $"Updated {(int)ts.TotalDays} day(s) ago";

            if (ts.TotalDays < 30)
                return $"Updated {(int)(ts.TotalDays / 7)} week(s) ago";

            if (ts.TotalDays < 365)
                return $"Updated {(int)(ts.TotalDays / 30)} month(s) ago";

            return $"Updated {(int)(ts.TotalDays / 365)} year(s) ago";
        }
    }
}
