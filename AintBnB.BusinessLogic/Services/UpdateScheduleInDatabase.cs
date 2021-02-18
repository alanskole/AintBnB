using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.DependencyProviderFactory;
using AintBnB.Core.Models;
using System.Collections.Generic;

namespace AintBnB.BusinessLogic.Services
{
    static public class UpdateScheduleInDatabase
    {
        public static void UpdateTheDatesOfTheScheduleInTheDb(Booking booking, List<string> dates, SortedDictionary<string, bool> schedule)
        {
            foreach (string datesBooked in dates)
            {
                if (schedule.ContainsKey(datesBooked))
                    schedule[datesBooked] = true;
            }
            UpdateScheduleInDb(booking.Accommodation.Id, booking.Accommodation.Schedule);
        }

        public static void UpdateScheduleInDb(int id, SortedDictionary<string, bool> schedule)
        {
            var acc = ProvideDependencyFactory.databaseContext.Accommodation.Find(id);

            if (acc == null)
                throw new IdNotFoundException("Accommodation", id);

            acc.Schedule = new SortedDictionary<string, bool>(schedule);
            ProvideDependencyFactory.databaseContext.SaveChanges();
        }
    }
}
