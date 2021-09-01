using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Imp;
using AintBnB.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Test.Unit
{
    [TestClass]
    public class BookingServiceTest : TestBase
    {
        [TestInitialize]
        public async Task SetUp()
        {
            await SetupDatabaseForTestingAsync();
            SetupTestClasses();
            await CreateDummyUsersAsync();
            await CreateDummyAccommodationAsync();
        }

        [TestCleanup]
        public async Task TearDown()
        {
            await DisposeAsync();
        }

        [TestMethod]
        public async Task BookAsync_ShouldReturn_NewBookingIfDatesAreAvailable()
        {
            var startDate = DateTime.Today.ToString("yyyy-MM-dd");
            var nights = 2;

            var bk = await bookingService.BookAsync(startDate, userCustomer2, nights, accommodation1);

            Assert.AreEqual(nights * accommodation1.PricePerNight, bk.Price);
            Assert.AreEqual(nights, bk.Dates.Count);
            Assert.AreEqual(startDate, bk.Dates[0]);
            Assert.AreEqual(DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), bk.Dates[1]);
            Assert.AreEqual(userCustomer2.Id, bk.BookedBy.Id);
            Assert.AreEqual(accommodation1.Id, bk.Accommodation.Id);
        }

        [TestMethod]
        public async Task BookAsync_ShouldFail_IfOwnerOfAccommodationWantsToBookTheirOwnAccommodationAsync()
        {
            var startDate = DateTime.Today.ToString("yyyy-MM-dd");
            var nights = 2;

            var ex = await Assert.ThrowsExceptionAsync<ParameterException>(async ()
                => await bookingService.BookAsync(startDate, userCustomer1, nights, accommodation1));

            Assert.AreEqual("Accommodation cannot be booked by the owner!", ex.Message);
        }

        [TestMethod]
        public async Task BookAsync_ShouldFail_IfNightsAreLessThanOneAsync()
        {
            var startDate = DateTime.Today.ToString("yyyy-MM-dd");
            var nights = 0;

            var ex = await Assert.ThrowsExceptionAsync<ParameterException>(async ()
                => await bookingService.BookAsync(startDate, userCustomer2, nights, accommodation1));

            Assert.AreEqual("Nights cannot be less than one!", ex.Message);
        }

        [TestMethod]
        public async Task TryToBookIfAllDatesAvailable_ShouldFail_IfAllDatesAreNotAvailable()
        {
            await CreateDummyBookingAsync();

            var startDate = booking1.Dates[booking1.Dates.Count - 1];
            var nights = 2;

            var result = typeof(BookingService)
                .GetMethod("TryToBookIfAllDatesAvailable", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.ThrowsException<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { startDate, userCustomer2, nights, accommodation1 }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(DateException));

            Assert.AreEqual("Dates aren't available", ex.InnerException.Message);
        }

        [TestMethod]
        public void TryToBookIfAllDatesAvailable_ShouldPass_IfAllDatesAreAvailable()
        {
            var startDate = DateTime.Today.ToString("yyyy-MM-dd");

            var nights = 2;

            var result = typeof(BookingService)
                .GetMethod("TryToBookIfAllDatesAvailable", BindingFlags.NonPublic | BindingFlags.Instance);

            result.Invoke(bookingService, new object[] { startDate, userCustomer2, nights, accommodation1 });
        }

        [TestMethod]
        public async Task UpdateBookingAsync_ShouldSucceed_IfNewDatesAreNotBookedByOthers_CaseWhenNewStartDateIsBeforeOriginal_AndNewCheckoutDateIsAfterOriginal()
        {
            await CreateDummyBookingAsync();

            var newStartDateTime = DateTime.Today.AddDays(1);
            var originalStartDateTime = DateTime.Parse(booking1.Dates[0]);
            var originalCheckoutDateTime = DateTime.Parse(booking1.Dates[booking1.Dates.Count - 1]);
            var originalPrice = booking1.Price;
            var orignalDates = booking1.Dates;

            var newStartDate = newStartDateTime.ToString("yyyy-MM-dd");
            var nights = 7;

            await bookingService.UpdateBookingAsync(newStartDate, nights, booking1.Id);

            var newCheckOutDateTime = DateTime.Parse(booking1.Dates[booking1.Dates.Count - 1]);
            var newCheckOutDate = newCheckOutDateTime.ToString("yyyy-MM-dd");

            Assert.AreEqual(nights, booking1.Dates.Count);
            Assert.AreEqual(newStartDate, booking1.Dates[0]);
            Assert.AreEqual(newCheckOutDate, booking1.Dates[booking1.Dates.Count - 1]);
            Assert.AreEqual(newStartDateTime.AddDays(1), originalStartDateTime);
            Assert.AreEqual(originalCheckoutDateTime.AddDays(1), newCheckOutDateTime);
            Assert.AreEqual(originalPrice + (booking1.Accommodation.PricePerNight * 2), booking1.Price);
            Assert.AreEqual(orignalDates[0], booking1.Dates[1]);
            Assert.AreEqual(orignalDates[1], booking1.Dates[2]);
            Assert.AreEqual(orignalDates[2], booking1.Dates[3]);
            Assert.AreEqual(orignalDates[3], booking1.Dates[4]);
            Assert.AreEqual(orignalDates[4], booking1.Dates[5]);
        }

        [TestMethod]
        public async Task UpdateBookingAsync_Should_SetUpdatedDatesToUnavailableInSchedule()
        {
            await CreateDummyBookingAsync();

            var newStartDateTime = DateTime.Today.AddDays(1);
            var originalStartDateTime = DateTime.Parse(booking1.Dates[0]);
            var originalCheckoutDateTime = DateTime.Parse(booking1.Dates[booking1.Dates.Count - 1]);
            int originalPrice = booking1.Price;
            var orignalDates = booking1.Dates;

            var newStartDate = newStartDateTime.ToString("yyyy-MM-dd");
            var nights = 7;

            await bookingService.UpdateBookingAsync(newStartDate, nights, booking1.Id);

            var newCheckOutDateTime = DateTime.Parse(booking1.Dates[booking1.Dates.Count - 1]);
            var newCheckOutDate = newCheckOutDateTime.ToString("yyyy-MM-dd");


            var result = typeof(BookingService)
                .GetMethod("TryToBookIfAllDatesAvailable", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.ThrowsException<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { newStartDate, userCustomer2, 1, accommodation1 }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(DateException));

            Assert.AreEqual("Dates aren't available", ex.InnerException.Message);

            ex = Assert.ThrowsException<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { newCheckOutDate, userCustomer2, 1, accommodation1 }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(DateException));

            Assert.AreEqual("Dates aren't available", ex.InnerException.Message);

            ex = Assert.ThrowsException<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { orignalDates[0], userCustomer2, orignalDates.Count, accommodation1 }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(DateException));

            Assert.AreEqual("Dates aren't available", ex.InnerException.Message);
        }

        [TestMethod]
        public async Task UpdateBookingAsync_ShouldFail_WhenCheckOutDateOfOriginalBookingIsInThePast()
        {
            await CreateDummyBookingAsync();

            var newStartDateTime = DateTime.Today.AddDays(10);
            var orignalDates = booking4.Dates;

            var newStartDate = newStartDateTime.ToString("yyyy-MM-dd");

            var ex = await Assert.ThrowsExceptionAsync<DateException>(async ()
                => await bookingService.UpdateBookingAsync(newStartDate, 5, booking4.Id));

            Assert.AreEqual("Can't update a booking that has a checkout date that's in the past!", ex.Message);


            Assert.AreEqual(orignalDates.Count, booking4.Dates.Count);
            Assert.AreEqual(orignalDates, booking4.Dates);
        }

        [TestMethod]
        public async Task UpdateBookingAsync_ShouldSucceed_AndSetTheOriginalDatesThatAreNotBookedAnyLongerToAvailable_CaseWhenNewCheckOutDateIsBeforeOriginal()
        {
            await CreateDummyBookingAsync();

            var newStartDateTime = DateTime.Today.AddDays(1);
            var originalStartDateTime = DateTime.Parse(booking1.Dates[0]);
            var originalCheckoutDateTime = DateTime.Parse(booking1.Dates[booking1.Dates.Count - 1]);
            var orignalDates = booking1.Dates;

            var newStartDate = newStartDateTime.ToString("yyyy-MM-dd");
            var nights = 5;

            await bookingService.UpdateBookingAsync(newStartDate, nights, booking1.Id);

            var newCheckOutDateTime = DateTime.Parse(booking1.Dates[booking1.Dates.Count - 1]);
            var newCheckOutDate = newCheckOutDateTime.ToString("yyyy-MM-dd");

            Assert.AreEqual(nights, booking1.Dates.Count);
            Assert.AreEqual(newStartDate, booking1.Dates[0]);
            Assert.AreEqual(newCheckOutDate, booking1.Dates[booking1.Dates.Count - 1]);
            Assert.AreEqual(newStartDateTime.AddDays(1), originalStartDateTime);
            Assert.AreEqual(newCheckOutDateTime.AddDays(1), originalCheckoutDateTime);
            Assert.AreEqual(orignalDates[0], booking1.Dates[1]);
            Assert.AreEqual(orignalDates[1], booking1.Dates[2]);
            Assert.AreEqual(orignalDates[2], booking1.Dates[3]);
            Assert.AreEqual(orignalDates[3], booking1.Dates[4]);
            Assert.IsFalse(booking1.Dates.Contains(orignalDates[4]));
            Assert.AreEqual(true, booking1.Accommodation.Schedule[orignalDates[orignalDates.Count - 1]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[newCheckOutDate]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[orignalDates[0]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[orignalDates[1]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[orignalDates[2]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[orignalDates[3]]);
        }

        [TestMethod]
        public async Task UpdateBookingAsync_ShouldSucceed_AndSetTheOriginalDatesThatAreNotBookedAnyLongerToAvailable_CaseWhenNewStartDateIsAfterOriginal()
        {
            await CreateDummyBookingAsync();

            var newStartDateTime = DateTime.Today.AddDays(3);
            var originalStartDateTime = DateTime.Parse(booking1.Dates[0]);
            var originalCheckoutDateTime = DateTime.Parse(booking1.Dates[booking1.Dates.Count - 1]);
            var orignalDates = booking1.Dates;

            var newStartDate = newStartDateTime.ToString("yyyy-MM-dd");
            var nights = 4;

            await bookingService.UpdateBookingAsync(newStartDate, nights, booking1.Id);

            var newCheckOutDateTime = DateTime.Parse(booking1.Dates[booking1.Dates.Count - 1]);
            var newCheckOutDate = newCheckOutDateTime.ToString("yyyy-MM-dd");

            Assert.AreEqual(nights, booking1.Dates.Count);
            Assert.AreEqual(newStartDate, booking1.Dates[0]);
            Assert.AreEqual(newCheckOutDate, booking1.Dates[booking1.Dates.Count - 1]);
            Assert.AreEqual(originalStartDateTime.AddDays(1), newStartDateTime);
            Assert.AreEqual(newCheckOutDateTime, originalCheckoutDateTime);
            Assert.AreEqual(orignalDates[1], booking1.Dates[0]);
            Assert.AreEqual(orignalDates[2], booking1.Dates[1]);
            Assert.AreEqual(orignalDates[3], booking1.Dates[2]);
            Assert.AreEqual(orignalDates[4], booking1.Dates[3]);
            Assert.IsFalse(booking1.Dates.Contains(orignalDates[0]));
            Assert.AreEqual(true, booking1.Accommodation.Schedule[orignalDates[0]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[orignalDates[1]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[orignalDates[2]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[orignalDates[3]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[orignalDates[4]]);
        }

        [TestMethod]
        public async Task UpdateBookingAsync_ShouldSucceed_AndUpdateOldDatesToAvailableAndNewDatesToUnavailable_CaseWhenAllTheNewDatesAreDifferentThanOriginal()
        {
            await CreateDummyBookingAsync();

            var newStartDateTime = DateTime.Today.AddDays(15);
            var orignalDates = booking1.Dates;

            var newStartDate = newStartDateTime.ToString("yyyy-MM-dd");
            var nights = 5;

            await bookingService.UpdateBookingAsync(newStartDate, nights, booking1.Id);


            Assert.IsFalse(booking1.Dates.Contains(orignalDates[0]));
            Assert.IsFalse(booking1.Dates.Contains(orignalDates[1]));
            Assert.IsFalse(booking1.Dates.Contains(orignalDates[2]));
            Assert.IsFalse(booking1.Dates.Contains(orignalDates[3]));
            Assert.IsFalse(booking1.Dates.Contains(orignalDates[4]));
            Assert.AreEqual(true, booking1.Accommodation.Schedule[orignalDates[0]]);
            Assert.AreEqual(true, booking1.Accommodation.Schedule[orignalDates[1]]);
            Assert.AreEqual(true, booking1.Accommodation.Schedule[orignalDates[2]]);
            Assert.AreEqual(true, booking1.Accommodation.Schedule[orignalDates[3]]);
            Assert.AreEqual(true, booking1.Accommodation.Schedule[orignalDates[4]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[booking1.Dates[0]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[booking1.Dates[1]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[booking1.Dates[2]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[booking1.Dates[3]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[booking1.Dates[4]]);
        }

        [TestMethod]
        public async Task GetBookingAsync_ShouldReturn_CorrectBooking()
        {
            await CreateDummyBookingAsync();

            var bk = await bookingService.GetBookingAsync(1);

            Assert.AreEqual(booking1.Id, bk.Id);
        }

        [TestMethod]
        public async Task GetBookingAsync_ShouldFail_IfIdDoesNotExist()
        {
            await CreateDummyBookingAsync();

            var ex = await Assert.ThrowsExceptionAsync<IdNotFoundException>(async ()
                => await bookingService.GetBookingAsync(100));

            Assert.AreEqual("Booking with ID 100 not found!", ex.Message);
        }

        [TestMethod]
        public async Task GetBookingsOfOwnedAccommodationAsync_ShouldReturn_ListOfAllBookingsOnTheAccommodationOfTheUser()
        {
            await CreateDummyBookingAsync();

            var all = await bookingService.GetBookingsOfOwnedAccommodationAsync(3);

            Assert.AreEqual(2, all.Count);
            Assert.AreEqual(booking2.Id, all[0].Id);
            Assert.AreEqual(booking3.Id, all[1].Id);
            Assert.AreEqual(booking2.Accommodation.Owner, userCustomer2);
            Assert.AreEqual(booking3.Accommodation.Owner, userCustomer2);
        }

        [TestMethod]
        public async Task GetBookingsOfOwnedAccommodationAsync_ShouldFail_IfThereAreNoBookingsOnTheAccommodationsOfTheUser()
        {
            await CreateDummyBookingAsync();

            await userService.CreateUserAsync("usr", "aaaaaa", "fa", "sd", UserTypes.Customer);

            adr1.Id = 100;

            var user = await userService.GetUserAsync(5);

            await accommodationService.CreateAccommodationAsync(user, adr1, 1, 2, 1, "d", 1, 1, 10);

            var ex = await Assert.ThrowsExceptionAsync<NoneFoundInDatabaseTableException>(async ()
                => await bookingService.GetBookingsOfOwnedAccommodationAsync(user.Id));

            Assert.AreEqual($"User with Id {user.Id} doesn't have any bookings of owned accommodations!", ex.Message);
        }

        [TestMethod]
        public async Task GetAllBookingsAsync_ShouldReturn_AllBookingsInTheSystem()
        {
            await CreateDummyBookingAsync();

            var allBookings = await bookingService.GetAllInSystemAsync();

            Assert.AreEqual(6, allBookings.Count);
            Assert.IsTrue(allBookings.Contains(booking1));
            Assert.IsTrue(allBookings.Contains(booking2));
            Assert.IsTrue(allBookings.Contains(booking3));
        }

        [TestMethod]
        public async Task GetAllBookingsAsync_ShouldReturn_AllBookingsOfTheCustomer()
        {
            await CreateDummyBookingAsync();

            var allBookings = await bookingService.GetOnlyOnesOwnedByUserAsync(userCustomer1.Id);

            Assert.AreEqual(2, allBookings.Count);
            Assert.IsTrue(allBookings.Contains(booking2));
            Assert.IsTrue(allBookings.Contains(booking3));
        }

        [TestMethod]
        public async Task GetAllBookingsAsync_ShouldFail_IfThereAreNoBookingsInTheSystemAsync()
        {
            var ex = await Assert.ThrowsExceptionAsync<NoneFoundInDatabaseTableException>(async ()
                => await bookingService.GetAllInSystemAsync());

            Assert.AreEqual("No bookings found!", ex.Message);
        }

        [TestMethod]
        public async Task GetAllBookingsAsync_ShouldFail_IfCustomerDoesNotHaveAnyBookingsAsync()
        {
            var ex = await Assert.ThrowsExceptionAsync<NoneFoundInDatabaseTableException>(async ()
                => await bookingService.GetAllInSystemAsync());

            Assert.AreEqual($"No bookings found!", ex.Message);
        }

        [TestMethod]
        public async Task CanRatingBeGiven_ShouldFail_WhenTryingToRateBeforeTheCheckoutDateHasPassed()
        {
            await CreateDummyBookingAsync();

            var result = typeof(BookingService)
                .GetMethod("CanRatingBeGiven", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.ThrowsException<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { booking1, 3 }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(ParameterException));

            Assert.AreEqual("Rating cannot be given until after checking out!", ex.InnerException.Message);
        }

        [TestMethod]
        public async Task CanRatingBeGiven_ShouldFail_WhenBookingHasAlreadyBeenRated()
        {
            await CreateDummyBookingAsync();

            booking1.Rating = 2;

            var result = typeof(BookingService)
                .GetMethod("CanRatingBeGiven", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.ThrowsException<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { booking1, 3 }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(ParameterException));

            Assert.AreEqual("Rating cannot be given twice!", ex.InnerException.Message);
        }

        [TestMethod]
        public async Task CanRatingBeGiven_ShouldFail_IfRatingIsLessThanOneOrHigherThanFive()
        {
            await CreateDummyBookingAsync();

            var result = typeof(BookingService)
                .GetMethod("CanRatingBeGiven", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.ThrowsException<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { booking1, 0 }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(ParameterException));

            Assert.AreEqual("Rating cannot be less than 1 or bigger than 5!", ex.InnerException.Message);

            result = typeof(BookingService)
                .GetMethod("CanRatingBeGiven", BindingFlags.NonPublic | BindingFlags.Instance);

            ex = Assert.ThrowsException<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { booking1, 6 }));

            Assert.AreEqual("Rating cannot be less than 1 or bigger than 5!", ex.InnerException.Message);
        }
    }
}
