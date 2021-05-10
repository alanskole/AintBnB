using System;
using System.Collections.Generic;

namespace AintBnB.BusinessLogic.Helpers
{
    internal static class DateHelper
    {

        /// <summary>Parses a datetime to a string.</summary>
        /// <param name="date">The date that must be parsed to a string.</param>
        /// <returns>A string with the datetime</returns>
        public static String DateFormatterCustomDate(DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }

        /// <summary>Checks if a set of dates are withing the within range of a schedule.</summary>
        /// <param name="schedule">The sorted dictionary that acts as the schedule.</param>
        /// <param name="fromDate">From date to check from.</param>
        /// <param name="nights">The amount of nights.</param>
        /// <returns>True if the dates are within the range of the schedule, false otherwise</returns>
        public static bool AreDatesWithinRangeOfSchedule(SortedDictionary<string, bool> schedule, string fromDate, int nights)
        {
            var lastDate = DateFormatterCustomDate(DateTime.Parse(fromDate).AddDays(nights - 1));

            if (DateIsInThePast(fromDate) || !schedule.ContainsKey(fromDate) || !schedule.ContainsKey(lastDate))
                return false;

            return true;
        }

        /// <summary>Checks if a date is in the past.</summary>
        /// <param name="date">The date to check.</param>
        /// <returns>True if the date is in the past, false otherwise</returns>
        public static bool DateIsInThePast(string date)
        {
            var dateToCheck = DateTime.Parse(date);
            if (dateToCheck >= DateTime.Today)
                return false;
            return true;
        }

        /// <summary>Checks if the cancelations date deadline has surpassed.</summary>
        /// <param name="firstDateBooked">The startdate ofa booking.</param>
        /// <param name="deadlineInDays">The cancellation deadline in days.</param>
        /// <returns>True if the date can be cancelled, false if the cancellation deadline has expired</returns>
        public static bool CancelationDeadlineCheck(string firstDateBooked, int deadlineInDays)
        {
            deadlineInDays -= 1;

            if (DateTime.Today < DateTime.Parse(firstDateBooked).AddDays(-deadlineInDays))
                return true;
            else
                return false;
        }

        /// <summary>Ares all dates available in the schedule.</summary>
        /// <param name="schedule">The schedule.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="nights">The amount of nights.</param>
        /// <returns>True if all the dates are available, false otherwise</returns>
        public static bool AreAllDatesAvailable(SortedDictionary<string, bool> schedule, string fromDate, int nights)
        {
            if (AreDatesWithinRangeOfSchedule(schedule, fromDate, nights))
            {
                var from = DateTime.Parse(fromDate);

                for (int i = 0; i < nights; i++)
                {

                    if (schedule[DateFormatterCustomDate(from.AddDays(i))] == false)
                        return false;
                }

                return true;
            }
            return false;
        }

        /// <summary>Gets the unavailable dates.</summary>
        /// <param name="schedule">The schedule.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="nights">The amount of nights.</param>
        /// <returns>A list with the unavailable dates</returns>
        public static List<string> GetUnavailableDates(SortedDictionary<string, bool> schedule, string fromDate, int nights)
        {
            var unavailableDates = new List<string>();

            if (AreDatesWithinRangeOfSchedule(schedule, fromDate, nights))
            {
                var from = DateTime.Parse(fromDate);

                for (int i = 0; i < nights; i++)
                {
                    if (schedule[DateFormatterCustomDate(from.AddDays(i))] == false)
                        unavailableDates.Add(DateFormatterCustomDate(from.AddDays(i)));
                }

            }
            return unavailableDates;
        }

        /// <summary>Resets the dates to available from unavailable.</summary>
        /// <param name="dates">The list with the dates to set to available.</param>
        /// <param name="schedule">The schedule where the dates must be set to available.</param>
        public static void ResetDatesToAvailable(List<string> dates, SortedDictionary<string, bool> schedule)
        {
            foreach (string datesBooked in dates)
            {
                if (schedule.ContainsKey(datesBooked))
                    schedule[datesBooked] = true;
            }
        }
    }
}
