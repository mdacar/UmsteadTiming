using System;

namespace UltimateTiming.DomainModel.Utility
{
    public class DateUtility
    {
        public static DateTime CurrentDate()
        {
            return ConvertUniversalTimeToEastern(DateTime.Now);
        }

        public static DateTime ConvertEasternTimeToUniversal(DateTime easternDateTime)
        {

            TimeZoneInfo easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            TimeSpan utcOffset = easternTimeZone.GetUtcOffset(easternDateTime);
            return new DateTime(easternDateTime.Subtract(utcOffset).Ticks, DateTimeKind.Utc);

        }

        public static DateTime ConvertUniversalTimeToEastern(DateTime utcDateTime)
        {
            if (utcDateTime.Kind == DateTimeKind.Unspecified)
            {
                utcDateTime = new DateTime(utcDateTime.Ticks, DateTimeKind.Utc);
            }
            TimeZoneInfo easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            TimeSpan utcOffset = easternTimeZone.GetUtcOffset(utcDateTime);
            return new DateTime(utcDateTime.Add(utcOffset).Ticks, DateTimeKind.Local);

        }

        public static DateTime SetAsEasternTime(DateTime date)
        {
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime universalDate = DateUtility.ConvertEasternTimeToUniversal(date);
            return TimeZoneInfo.ConvertTimeFromUtc(universalDate, tzi);
        }
    }
}
