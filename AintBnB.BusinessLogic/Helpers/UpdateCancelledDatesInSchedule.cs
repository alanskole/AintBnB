using System.Collections.Generic;

namespace AintBnB.BusinessLogic.Helpers
{
    static public class UpdateCancelledDatesInSchedule
    {
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
