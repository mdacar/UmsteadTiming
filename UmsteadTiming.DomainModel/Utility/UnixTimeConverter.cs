using System;
using System.Text;
using System.Threading;

namespace UltimateTiming.Domain
{
    public class UnixTimeConverter
    {
        private static DateTime Jan1_1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime ConvertUnixTimeStamp(string p)
        {

            // Unix timestamp is milliseconds past epoch
            System.DateTime dtDateTime = Jan1_1970;
            dtDateTime = dtDateTime.AddMilliseconds(Convert.ToDouble(p) / 1000);
            return dtDateTime;

        }



        public static string GetCurrentUnixTime()
        {
            string unixTime = ToLongString((DateTime.UtcNow - Jan1_1970).TotalMilliseconds * 1000);
            return unixTime;
        }

        public static string GetUnixTime(DateTime date)
        {
            string unixTime = ToLongString((date - Jan1_1970).TotalMilliseconds * 1000);
            return unixTime;
        }

        public static long GetUnixTimeLong(DateTime date)
        {
            double unixTime = ((date - Jan1_1970).TotalMilliseconds * 1000);
            return (long)unixTime;
        }


        private static string ToLongString(double input)
        {
            string str = input.ToString().ToUpper();

            // if string representation was collapsed from scientific notation, just return it:
            if (!str.Contains("E")) return str;

            bool negativeNumber = false;

            if (str[0] == '-')
            {
                str = str.Remove(0, 1);
                negativeNumber = true;
            }

            string sep = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            char decSeparator = sep.ToCharArray()[0];

            string[] exponentParts = str.Split('E');
            string[] decimalParts = exponentParts[0].Split(decSeparator);

            // fix missing decimal point:
            if (decimalParts.Length == 1) decimalParts = new string[] { exponentParts[0], "0" };

            int exponentValue = int.Parse(exponentParts[1]);

            string newNumber = decimalParts[0] + decimalParts[1];

            string result;

            if (exponentValue > 0)
            {
                result =
                    newNumber +
                    GetZeros(exponentValue - decimalParts[1].Length);
            }
            else // negative exponent
            {
                result =
                    "0" +
                    decSeparator +
                    GetZeros(exponentValue + decimalParts[0].Length) +
                    newNumber;

                result = result.TrimEnd('0');
            }

            if (negativeNumber)
                result = "-" + result;

            return result;
        }

        private static string GetZeros(int zeroCount)
        {
            if (zeroCount < 0)
                zeroCount = Math.Abs(zeroCount);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < zeroCount; i++) sb.Append("0");

            return sb.ToString();
        }


        public static string GetUnixTimeLocal(DateTime localTime)
        {
            string unixTime = ToLongString((localTime - Jan1_1970).TotalMilliseconds * 1000);
            return unixTime;
        }


    }
}
