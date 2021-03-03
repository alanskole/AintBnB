using System;
using System.Collections.Generic;

namespace AintBnB.BusinessLogic.Helpers
{
    public static class DateHelper
    {
        public static String DateFormatterTodaysDate()
        {
            return DateTime.Today.ToString("yyyy-MM-dd");
        }

        public static String DateFormatterCustomDate(DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }

        public static bool AreDatesWithinRangeOfSchedule(SortedDictionary<string, bool> schedule, string fromDate, int nights)
        {
            string lastDate = DateFormatterCustomDate(DateTime.Parse(fromDate).AddDays(nights-1));

            if (StartDateIsInThePast(fromDate) || !schedule.ContainsKey(fromDate) || !schedule.ContainsKey(lastDate))
                return false;

            return true;
        }

        private static bool StartDateIsInThePast(string startDate)
        {
            DateTime dateToCheck = DateTime.Parse(startDate);
            if (dateToCheck >= DateTime.Today)
                return false;
            return true;
        }

        public static bool CancelationDeadlineCheck(string firstDateBooked, int deadlineInDays)
        {
            deadlineInDays -= 1;

            if (DateTime.Today < DateTime.Parse(firstDateBooked).AddDays(-deadlineInDays))
                return true;
            else
                return false;
        }

        public static bool AreAllDatesAvailable(SortedDictionary<string, bool> schedule, string fromDate, int nights)
        {
            if (AreDatesWithinRangeOfSchedule(schedule, fromDate, nights))
            {
                DateTime from = DateTime.Parse(fromDate);

                for (int i = 0; i < nights; i++)
                {

                    if (schedule[DateFormatterCustomDate(from.AddDays(i))] == false)
                        return false;
                }

                return true;
            }
            return false;
        }

        public static List<string> GetUnavailableDates(SortedDictionary<string, bool> schedule, string fromDate, int nights)
        {
            List<string> unavailableDates = new List<string>();

            if (AreDatesWithinRangeOfSchedule(schedule, fromDate, nights))
            {
                DateTime from = DateTime.Parse(fromDate);

                for (int i = 0; i < nights; i++)
                {
                    if (schedule[DateFormatterCustomDate(from.AddDays(i))] == false)
                        unavailableDates.Add(DateFormatterCustomDate(from.AddDays(i)));
                }

            }
            return unavailableDates;
        }
    }
}
