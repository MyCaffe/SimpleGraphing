using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class TimeZoneEx
    {
        public static double GetTimeZoneOffset()
        {
            TimeZone zone = TimeZone.CurrentTimeZone;
            TimeZoneInfo eastern;
            DateTime dtNow = DateTime.Now;
            eastern = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            TimeSpan localOffset = zone.GetUtcOffset(dtNow);
            TimeSpan easternOffset = eastern.GetUtcOffset(dtNow);
            TimeSpan diff = easternOffset - localOffset;
            return diff.TotalHours;
        }
    }
}
