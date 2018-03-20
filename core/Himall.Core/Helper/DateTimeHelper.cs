using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Core.Helper
{
    public class DateTimeHelper
    {
        public static DateTime GetStartDayOfWeeks(int year, int month,int index)
        {
            if (year < 1600 || year > 9999)
            {
                return DateTime.MinValue;
            }
            if (month < 0 || month > 12)
            {
                return DateTime.MinValue;
            }
            if(index<1)
            {
                return DateTime.MinValue;
            }
            DateTime startMonth = new DateTime(year, month, 1);  //该月第一天  
            int dayOfWeek = 7;
            if (Convert.ToInt32(startMonth.DayOfWeek.ToString("d")) > 0)
                dayOfWeek = Convert.ToInt32(startMonth.DayOfWeek.ToString("d"));  //该月第一天为星期几  
            DateTime startWeek = startMonth.AddDays(1 - dayOfWeek);  //该月第一周开始日期  
            //DateTime startDayOfWeeks = startWeek.AddDays((index - 1) * 7);  //index周的起始日期  
            DateTime startDayOfWeeks = startWeek.AddDays(index * 7);  //index周的起始日期  
            if ((startDayOfWeeks - startMonth.AddMonths(1)).Days > 0)  //startDayOfWeeks不在该月范围内  
            {
                return DateTime.MinValue;
            }

            return startDayOfWeeks;
        }

        public static string GetWeekSpanOfMonth(int year, int month)
        {
            if (year < 1600 || year > 9999)
            {
                return "";
            }
            if (month < 0 || month > 12)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            for (int index = 1; index < 5; index++)
            {
                DateTime startMonth = new DateTime(year, month, 1);  //该月第一天  
                int dayOfWeek = 7;
                if (Convert.ToInt32(startMonth.DayOfWeek.ToString("d")) > 0)
                    dayOfWeek = Convert.ToInt32(startMonth.DayOfWeek.ToString("d"));  //该月第一天为星期几  
                DateTime startWeek = startMonth.AddDays(1 - dayOfWeek);  //该月第一周开始日期  
                //DateTime startDayOfWeeks = startWeek.AddDays((index - 1) * 7);  //index周的起始日期  
                DateTime startDayOfWeeks = startWeek.AddDays(index * 7);  //index周的起始日期  
                if ((startDayOfWeeks - startMonth.AddMonths(1)).Days > 0)  //startDayOfWeeks不在该月范围内  
                {
                    return "";
                }
                sb.Append(startDayOfWeeks.ToString("yyyy-MM-dd"));
                sb.Append(" ~ ");
                sb.Append(startDayOfWeeks.AddDays(6).ToString("yyyy-MM-dd"));
                sb.Append(Environment.NewLine);

            }
            return sb.ToString();
        }
    }
}
