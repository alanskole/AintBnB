using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using static AintBnB.BusinessLogic.Helpers.DateHelper;

namespace Test.Unit
{
    [TestClass]
    public class DateHelperTest
    {
        [TestMethod]
        public static void AreDatesWithinRangeOfSchedule_ShouldReturn_TrueWhenStartDateAndEndDateAreBetweenTheFirstAndLastKeysOfASchedule()
        {
            SortedDictionary<string, bool> schd = new SortedDictionary<string, bool>
            {
                { DateTime.Today.ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(2).ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(4).ToString("yyyy-MM-dd"), true }
            };

            Assert.IsTrue(AreDatesWithinRangeOfSchedule(schd, DateTime.Today.ToString("yyyy-MM-dd"), 5));
            Assert.IsTrue(AreDatesWithinRangeOfSchedule(schd, DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), 3));
            Assert.IsTrue(AreDatesWithinRangeOfSchedule(schd, DateTime.Today.AddDays(4).ToString("yyyy-MM-dd"), 1));
        }

        [TestMethod]
        public static void AreDatesWithinRangeOfSchedule_ShouldReturn_FalseWhenStartDateAndEndDateAreNotBetweenTheFirstAndLastKeysOfASchedule()
        {
            SortedDictionary<string, bool> schd = new SortedDictionary<string, bool>
            {
                { DateTime.Today.ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(2).ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(4).ToString("yyyy-MM-dd"), true }
            };
            Assert.IsFalse(AreDatesWithinRangeOfSchedule(schd, DateTime.Today.ToString("yyyy-MM-dd"), 6));
            Assert.IsFalse(AreDatesWithinRangeOfSchedule(schd, DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"), 3));
            Assert.IsFalse(AreDatesWithinRangeOfSchedule(schd, DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), 3));
            Assert.IsFalse(AreDatesWithinRangeOfSchedule(schd, DateTime.Today.AddDays(5).ToString("yyyy-MM-dd"), 1));
        }

        [TestMethod]
        public void StartDateIsInThePast_ShouldReturn_TrueWhenCheckingADateInThePast()
        {
            var res = DateIsInThePast(DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"));

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void DateIsInThePast_ShouldReturn_FalseWhenCheckingADateNotInThePast()
        {
            var res = DateIsInThePast(DateTime.Today.ToString("yyyy-MM-dd"));

            Assert.IsFalse(res);
        }

        [TestMethod]
        public void CancelationDeadlineCheck_ShouldReturn_TrueWhenStartDateIsNotAfterDeadline()
        {
            string startDate1 = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            string startDate2 = DateTime.Today.AddDays(2).ToString("yyyy-MM-dd");
            int cancellationDeadline = 1;

            Assert.IsTrue(CancelationDeadlineCheck(startDate1, cancellationDeadline));
            Assert.IsTrue(CancelationDeadlineCheck(startDate2, cancellationDeadline));
        }

        [TestMethod]
        public void CancelationDeadlineCheck_ShouldReturn_FalseWhenStartDateIsAfterDeadline()
        {
            string startDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            int cancellationDeadline = 2;

            Assert.IsFalse(CancelationDeadlineCheck(startDate, cancellationDeadline));
        }

        [TestMethod]
        public void AreAllDatesAvailable_ShouldReturn_TrueWhenAllDatesWithinThePeriodAreAvailable()
        {
            SortedDictionary<string, bool> schd = new SortedDictionary<string, bool>
            {
                { DateTime.Today.ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(2).ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(4).ToString("yyyy-MM-dd"), true }
            };

            string startDate = DateTime.Today.ToString("yyyy-MM-dd");
            string startDate2 = DateTime.Today.AddDays(2).ToString("yyyy-MM-dd");

            Assert.IsTrue(AreAllDatesAvailable(schd, startDate, 5));
            Assert.IsTrue(AreAllDatesAvailable(schd, startDate, 3));
        }

        [TestMethod]
        public void AreAllDatesAvailable_ShouldReturn_FalseWhenNotAllDatesWithinThePeriodAreAvailable()
        {
            SortedDictionary<string, bool> schd = new SortedDictionary<string, bool>
            {
                { DateTime.Today.ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(2).ToString("yyyy-MM-dd"), false },
                { DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(4).ToString("yyyy-MM-dd"), true }
            };

            string startDate = DateTime.Today.ToString("yyyy-MM-dd");
            string startDate2 = DateTime.Today.AddDays(2).ToString("yyyy-MM-dd");

            Assert.IsFalse(AreAllDatesAvailable(schd, startDate, 5));
            Assert.IsFalse(AreAllDatesAvailable(schd, startDate, 3));
        }

        [TestMethod]
        public void GetUnavailableDates_ShouldReturn_AListOfTheUnavailableDatesWithinThePeriod()
        {
            SortedDictionary<string, bool> schd = new SortedDictionary<string, bool>
            {
                { DateTime.Today.ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), false },
                { DateTime.Today.AddDays(2).ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"), false },
                { DateTime.Today.AddDays(4).ToString("yyyy-MM-dd"), true }
            };

            string startDate = DateTime.Today.ToString("yyyy-MM-dd");
            List<string> unav = GetUnavailableDates(schd, startDate, 5);

            Assert.AreEqual(2, unav.Count);
            Assert.AreEqual(DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), unav[0]);
            Assert.AreEqual(DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"), unav[1]);
        }

        [TestMethod]
        public void GetUnavailableDates_ShouldReturn_EmptyListIfAllDatesAreAvailable()
        {
            SortedDictionary<string, bool> schd = new SortedDictionary<string, bool>
            {
                { DateTime.Today.ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(2).ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"), true },
                { DateTime.Today.AddDays(4).ToString("yyyy-MM-dd"), true }
            };

            string startDate = DateTime.Today.ToString("yyyy-MM-dd");
            List<string> unav = GetUnavailableDates(schd, startDate, 5);

            Assert.AreEqual(0, unav.Count);
        }
    }
}
