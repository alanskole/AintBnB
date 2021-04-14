using AintBnB.BusinessLogic.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using static AintBnB.BusinessLogic.Helpers.DateHelper;

namespace Test.Unit
{
    [TestFixture]
    public class DateHelperTest
    {
        [Test]
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

            Assert.True(AreDatesWithinRangeOfSchedule(schd, DateTime.Today.ToString("yyyy-MM-dd"), 5));
            Assert.True(AreDatesWithinRangeOfSchedule(schd, DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), 3));
            Assert.True(AreDatesWithinRangeOfSchedule(schd, DateTime.Today.AddDays(4).ToString("yyyy-MM-dd"), 1));
        }

        [Test]
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
            Assert.False(AreDatesWithinRangeOfSchedule(schd, DateTime.Today.ToString("yyyy-MM-dd"), 6));
            Assert.False(AreDatesWithinRangeOfSchedule(schd, DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"), 3));
            Assert.False(AreDatesWithinRangeOfSchedule(schd, DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), 3));
            Assert.False(AreDatesWithinRangeOfSchedule(schd, DateTime.Today.AddDays(5).ToString("yyyy-MM-dd"), 1));
        }

        [Test]
        public void StartDateIsInThePast_ShouldReturn_TrueWhenCheckingADateInThePast()
        {
            var result = typeof(DateHelper)
                .GetMethod("StartDateIsInThePast", BindingFlags.NonPublic | BindingFlags.Static);

            bool res = (bool)result.Invoke(null, new object[] { DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd") });

            Assert.True(res);
        }

        [Test]
        public void StartDateIsInThePast_ShouldReturn_FalseWhenCheckingADateNotInThePast()
        {
            var result = typeof(DateHelper)
                .GetMethod("StartDateIsInThePast", BindingFlags.NonPublic | BindingFlags.Static);

            bool res = (bool)result.Invoke(null, new object[] { DateTime.Today.ToString("yyyy-MM-dd") });

            Assert.False(res);
        }

        [Test]
        public void CancelationDeadlineCheck_ShouldReturn_TrueWhenStartDateIsNotAfterDeadline()
        {
            string startDate1 = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            string startDate2 = DateTime.Today.AddDays(2).ToString("yyyy-MM-dd");
            int cancellationDeadline = 1;

            Assert.True(CancelationDeadlineCheck(startDate1, cancellationDeadline));
            Assert.True(CancelationDeadlineCheck(startDate2, cancellationDeadline));
        }

        [Test]
        public void CancelationDeadlineCheck_ShouldReturn_FalseWhenStartDateIsAfterDeadline()
        {
            string startDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            int cancellationDeadline = 2;

            Assert.False(CancelationDeadlineCheck(startDate, cancellationDeadline));
        }

        [Test]
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

            Assert.True(AreAllDatesAvailable(schd, startDate, 5));
            Assert.True(AreAllDatesAvailable(schd, startDate, 3));
        }

        [Test]
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

            Assert.False(AreAllDatesAvailable(schd, startDate, 5));
            Assert.False(AreAllDatesAvailable(schd, startDate, 3));
        }

        [Test]
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

        [Test]
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
