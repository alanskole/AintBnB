using AintBnB.BusinessLogic.DependencyProviderFactory;
using System.Collections.Generic;

namespace AintBnB.BusinessLogic.Services
{
    static public class UpdateScheduleInDatabase
    {
        public static void UpdateScheduleInDb(int id, SortedDictionary<string, bool> schedule)
        {
            var acc = ProvideDependencyFactory.databaseContext.Accommodation.Find(id);
            acc.Schedule = new SortedDictionary<string, bool>(schedule);
            ProvideDependencyFactory.databaseContext.SaveChanges();
        }
    }
}
