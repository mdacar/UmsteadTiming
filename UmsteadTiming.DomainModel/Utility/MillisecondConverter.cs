using System;

namespace UltimateTiming.DomainModel.Utility
{
    public class MillisecondConverter
    {
        public static string GetTimeFromMilliseconds(long milliseconds)
        {
            TimeSpan finalTimespan = TimeSpan.FromMilliseconds(milliseconds);
            int totalHours = (int)finalTimespan.TotalHours;
            int totalMinutes = (int)finalTimespan.Subtract(new TimeSpan(totalHours, 0, 0)).TotalMinutes;
            int totalSeconds = (int)finalTimespan.Subtract(new TimeSpan(totalHours, totalMinutes, 0)).TotalSeconds;

            return totalHours.ToString().PadLeft(2, '0') + ":" + totalMinutes.ToString().PadLeft(2, '0') + ":" + totalSeconds.ToString().PadLeft(2, '0');
        }

        public static string GetVerboseTimeFromMilliseconds(long milliseconds)
        {
            TimeSpan finalTimespan = TimeSpan.FromMilliseconds(milliseconds);
            int totalHours = (int)finalTimespan.TotalHours;
            int totalMinutes = (int)finalTimespan.Subtract(new TimeSpan(totalHours, 0, 0)).TotalMinutes;
            int totalSeconds = (int)finalTimespan.Subtract(new TimeSpan(totalHours, totalMinutes, 0)).TotalSeconds;

            return totalHours.ToString().PadLeft(2, '0') + "h " + totalMinutes.ToString().PadLeft(2, '0') + "m " + totalSeconds.ToString().PadLeft(2, '0') + "s";
        }
    }
}
