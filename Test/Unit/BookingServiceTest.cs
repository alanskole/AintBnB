using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Imp;
using AintBnB.Core.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace Test.Unit
{
    [TestFixture]
    public class BookingServiceTest : TestBase
    {
        [SetUp]
        public async Task SetUp()
        {
            await SetupDatabaseForTesting();
            SetupTestClasses();
            await CreateDummyUsers();
            await CreateDummyAccommodation();
        }

        [TearDown]
        public async Task TearDown()
        {
            await DisposeAsync();
        }

        [Test]
        public async Task Book_ShouldReturn_NewBookingIfDatesAreAvailable()
        {
            LoggedInAs = userCustomer2;

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

        [Test]
        public void Book_ShouldFail_IfOwnerOfAccommodationWantsToBookTheirOwnAccommodation()
        {
            LoggedInAs = userCustomer1;

            var startDate = DateTime.Today.ToString("yyyy-MM-dd");
            var nights = 2;

            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await bookingService.BookAsync(startDate, userCustomer1, nights, accommodation1));

            Assert.AreEqual("Accommodation cannot be booked by the owner!", ex.Message);
        }

        [Test]
        public void Book_ShouldFail_IfNightsAreLessThanOne()
        {
            LoggedInAs = userCustomer2;

            var startDate = DateTime.Today.ToString("yyyy-MM-dd");
            var nights = 0;

            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await bookingService.BookAsync(startDate, userCustomer2, nights, accommodation1));

            Assert.AreEqual("Nights cannot be less than one!", ex.Message);
        }

        [Test]
        public void BookIfAvailableAndUserHasPermission_ShouldFail_IfAdminWantsToBookForOwnAccount()
        {
            LoggedInAs = userAdmin;

            string startDate = DateTime.Today.ToString("yyyy-MM-dd");
            int nights = 2;

            var result = typeof(BookingService)
                            .GetMethod("BookIfAvailableAndUserHasPermission", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.Throws<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { startDate, userAdmin, nights, accommodation1 }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(AccessException));

            Assert.AreEqual($"Must be performed by a customer with ID {userAdmin.Id}, or by admin or an employee on behalf of a customer with ID {userAdmin.Id}!", ex.InnerException.Message);
        }

        [Test]
        public void BookIfAvailableAndUserHasPermission_ShouldFail_IfEmployeeWantsToBookForOwnAccount()
        {
            LoggedInAs = userEmployee1;

            var startDate = DateTime.Today.ToString("yyyy-MM-dd");
            var nights = 2;

            var result = typeof(BookingService)
                            .GetMethod("BookIfAvailableAndUserHasPermission", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.Throws<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { startDate, userEmployee1, nights, accommodation1 }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(AccessException));

            Assert.AreEqual($"Must be performed by a customer with ID {userEmployee1.Id}, or by admin or an employee on behalf of a customer with ID {userEmployee1.Id}!", ex.InnerException.Message);
        }

        [Test]
        public void BookIfAvailableAndUserHasPermission_ShouldFail_IfCustomerTriesToBookForAnotherCustomer()
        {
            LoggedInAs = userCustomer1;

            var startDate = DateTime.Today.ToString("yyyy-MM-dd");
            var nights = 2;

            var result = typeof(BookingService)
                            .GetMethod("BookIfAvailableAndUserHasPermission", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.Throws<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { startDate, userCustomer2, nights, accommodation1 }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(AccessException));

            Assert.AreEqual($"Must be performed by a customer with ID {userCustomer2.Id}, or by admin or an employee on behalf of a customer with ID {userCustomer2.Id}!", ex.InnerException.Message);
        }

        [Test]
        public void BookIfAvailableAndUserHasPermission_ShouldPass_IfAdminTriesToBookForACustomer()
        {
            LoggedInAs = userAdmin;

            var startDate = DateTime.Today.ToString("yyyy-MM-dd");
            var nights = 2;

            var result = typeof(BookingService)
                            .GetMethod("BookIfAvailableAndUserHasPermission", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.DoesNotThrow(()
                => result.Invoke(bookingService, new object[] { startDate, userCustomer2, nights, accommodation1 }));
        }

        [Test]
        public void BookIfAvailableAndUserHasPermission_ShouldPass_IfEmployeeTriesToBookForACustomer()
        {
            LoggedInAs = userEmployee1;

            var startDate = DateTime.Today.ToString("yyyy-MM-dd");
            var nights = 2;

            var result = typeof(BookingService)
                            .GetMethod("BookIfAvailableAndUserHasPermission", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.DoesNotThrow(()
                => result.Invoke(bookingService, new object[] { startDate, userCustomer2, nights, accommodation1 }));
        }

        [Test]
        public async Task TryToBookIfAllDatesAvailable_ShouldFail_IfAllDatesAreNotAvailable()
        {
            await CreateDummyBooking();

            var startDate = booking1.Dates[booking1.Dates.Count - 1];
            var nights = 2;

            var result = typeof(BookingService)
                            .GetMethod("TryToBookIfAllDatesAvailable", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.Throws<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { startDate, userCustomer2, nights, accommodation1 }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(DateException));

            Assert.AreEqual("Dates aren't available", ex.InnerException.Message);
        }

        [Test]
        public void TryToBookIfAllDatesAvailable_ShouldPass_IfAllDatesAreAvailable()
        {
            var startDate = DateTime.Today.ToString("yyyy-MM-dd");

            var nights = 2;

            var result = typeof(BookingService)
                            .GetMethod("TryToBookIfAllDatesAvailable", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.DoesNotThrow(()
                => result.Invoke(bookingService, new object[] { startDate, userCustomer2, nights, accommodation1 }));
        }

        [Test]
        public async Task UpdateBooking_ShouldSucceed_IfNewDatesAreNotBookedByOthers_CaseWhenNewStartDateIsBeforeOriginal_AndNewCheckoutDateIsAfterOriginal()
        {
            await CreateDummyBooking();

            LoggedInAs = booking1.BookedBy;

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

        [Test]
        public async Task UpdateBooking_Should_SetUpdatedDatesToUnavailableInSchedule()
        {
            await CreateDummyBooking();

            LoggedInAs = booking1.BookedBy;

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

            var ex = Assert.Throws<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { newStartDate, userCustomer2, 1, accommodation1 }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(DateException));

            Assert.AreEqual("Dates aren't available", ex.InnerException.Message);

            ex = Assert.Throws<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { newCheckOutDate, userCustomer2, 1, accommodation1 }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(DateException));

            Assert.AreEqual("Dates aren't available", ex.InnerException.Message);

            ex = Assert.Throws<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { orignalDates[0], userCustomer2, orignalDates.Count, accommodation1 }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(DateException));

            Assert.AreEqual("Dates aren't available", ex.InnerException.Message);
        }

        [Test]
        public async Task UpdateBooking_ShouldSucceed_AndSetTheOriginalDatesThatAreNotBookedAnyLongerToAvailable_CaseWhenNewCheckOutDateIsBeforeOriginal()
        {
            await CreateDummyBooking();

            LoggedInAs = booking1.BookedBy;

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
            Assert.False(booking1.Dates.Contains(orignalDates[4]));
            Assert.AreEqual(true, booking1.Accommodation.Schedule[orignalDates[orignalDates.Count - 1]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[newCheckOutDate]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[orignalDates[0]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[orignalDates[1]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[orignalDates[2]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[orignalDates[3]]);
        }

        [Test]
        public async Task UpdateBooking_ShouldSucceed_AndSetTheOriginalDatesThatAreNotBookedAnyLongerToAvailable_CaseWhenNewStartDateIsAfterOriginal()
        {
            await CreateDummyBooking();

            LoggedInAs = booking1.BookedBy;

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
            Assert.False(booking1.Dates.Contains(orignalDates[0]));
            Assert.AreEqual(true, booking1.Accommodation.Schedule[orignalDates[0]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[orignalDates[1]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[orignalDates[2]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[orignalDates[3]]);
            Assert.AreEqual(false, booking1.Accommodation.Schedule[orignalDates[4]]);
        }

        [Test]
        public async Task UpdateBooking_ShouldSucceed_AndUpdateOldDatesToAvailableAndNewDatesToUnavailable_CaseWhenAllTheNewDatesAreDifferentThanOriginal()
        {
            await CreateDummyBooking();

            LoggedInAs = booking1.BookedBy;

            var newStartDateTime = DateTime.Today.AddDays(15);
            var orignalDates = booking1.Dates;

            var newStartDate = newStartDateTime.ToString("yyyy-MM-dd");
            var nights = 5;

            await bookingService.UpdateBookingAsync(newStartDate, nights, booking1.Id);


            Assert.False(booking1.Dates.Contains(orignalDates[0]));
            Assert.False(booking1.Dates.Contains(orignalDates[1]));
            Assert.False(booking1.Dates.Contains(orignalDates[2]));
            Assert.False(booking1.Dates.Contains(orignalDates[3]));
            Assert.False(booking1.Dates.Contains(orignalDates[4]));
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

        [Test]
        public async Task GetBooking_ShouldReturn_CorrectBooking()
        {
            await CreateDummyBooking();

            LoggedInAs = userCustomer2;

            var bk = await bookingService.GetBookingAsync(1);

            Assert.AreEqual(booking1.Id, bk.Id);
        }

        [Test]
        public async Task GetBooking_ShouldReturn_CorrectBookingIfOwnerOfAccommodationTriesToGetABookingOfTheirAccommodationByAnotherUser()
        {
            await CreateDummyBooking();

            LoggedInAs = userCustomer1;

            var bk = await bookingService.GetBookingAsync(1);

            Assert.AreEqual(booking1.Id, bk.Id);
        }


        [Test]
        public async Task GetBooking_ShouldFail_IfIdDoesNotExist()
        {
            await CreateDummyBooking();

            LoggedInAs = userCustomer1;

            var ex = Assert.ThrowsAsync<IdNotFoundException>(async ()
                => await bookingService.GetBookingAsync(100));

            Assert.AreEqual("Booking with ID 100 not found!", ex.Message);
        }

        [Test]
        public async Task GetBooking_ShouldFail_IfUserThatIsNotOwnerOfAccommodationOrAdminOrEmployeeTriesToGetBookingOfAnotherUser()
        {
            await CreateDummyBooking();

            LoggedInAs = userRequestToBecomeEmployee;

            var ex = Assert.ThrowsAsync<AccessException>(async ()
                => await bookingService.GetBookingAsync(1));

            Assert.AreEqual("Restricted access!", ex.Message);
        }

        [Test]
        public async Task GetBookingsOfOwnedAccommodation_ShouldReturn_ListOfAllBookingsOnTheAccommodationOfTheUser()
        {
            await CreateDummyBooking();

            LoggedInAs = userCustomer2;

            var all = await bookingService.GetBookingsOfOwnedAccommodationAsync(6);

            Assert.AreEqual(2, all.Count);
            Assert.AreEqual(booking2.Id, all[0].Id);
            Assert.AreEqual(booking3.Id, all[1].Id);
            Assert.AreEqual(booking2.Accommodation.Owner, userCustomer2);
            Assert.AreEqual(booking3.Accommodation.Owner, userCustomer2);
        }

        [Test]
        public async Task GetBookingsOfOwnedAccommodation_ShouldFail_IfThereAreNoBookingsOnTheAccommodationsOfTheUser()
        {
            await CreateDummyBooking();

            LoggedInAs = userRequestToBecomeEmployee;

            userRequestToBecomeEmployee.UserType = UserTypes.Customer;

            adr.Id = 100;

            await accommodationService.CreateAccommodationAsync(userRequestToBecomeEmployee, adr, 1, 2, 1, "d", 1, 1, new List<byte[]>(), 10);

            var ex = Assert.ThrowsAsync<NoneFoundInDatabaseTableException>(async ()
                => await bookingService.GetBookingsOfOwnedAccommodationAsync(3));

            Assert.AreEqual($"User with Id {userRequestToBecomeEmployee.Id} doesn't have any bookings of owned accommodations!", ex.Message);
        }

        [Test]
        public async Task GetAllBookings_ShouldReturn_AllBookingsInTheSystemIfAdmin()
        {
            await CreateDummyBooking();

            LoggedInAs = userAdmin;

            var allBookings = await bookingService.GetAllBookingsAsync();

            Assert.AreEqual(3, allBookings.Count);
            Assert.True(allBookings.Contains(booking1));
            Assert.True(allBookings.Contains(booking2));
            Assert.True(allBookings.Contains(booking3));
        }

        [Test]
        public async Task GetAllBookings_ShouldReturn_AllBookingsInTheSystemIfEmployee()
        {
            await CreateDummyBooking();

            LoggedInAs = userEmployee1;

            var allBookings = await bookingService.GetAllBookingsAsync();

            Assert.AreEqual(3, allBookings.Count);
            Assert.True(allBookings.Contains(booking1));
            Assert.True(allBookings.Contains(booking2));
            Assert.True(allBookings.Contains(booking3));
        }

        [Test]
        public async Task GetAllBookings_ShouldReturn_AllBookingsOfTheCustomerIfNormalCustomer()
        {
            await CreateDummyBooking();

            LoggedInAs = userCustomer1;

            var allBookings = await bookingService.GetAllBookingsAsync();

            Assert.AreEqual(2, allBookings.Count);
            Assert.True(allBookings.Contains(booking2));
            Assert.True(allBookings.Contains(booking3));
        }

        [Test]
        public void GetAllBookings_ShouldFail_IfThereAreNoBookingsInTheSystem()
        {
            LoggedInAs = userAdmin;

            var ex = Assert.ThrowsAsync<NoneFoundInDatabaseTableException>(async ()
                => await bookingService.GetAllBookingsAsync());

            Assert.AreEqual("No bookings found!", ex.Message);
        }

        [Test]
        public void GetAllBookings_ShouldFail_IfCustomerDoesNotHaveAnyBookings()
        {
            LoggedInAs = userCustomer1;

            var ex = Assert.ThrowsAsync<NoneFoundInDatabaseTableException>(async ()
                => await bookingService.GetAllBookingsAsync());

            Assert.AreEqual($"User with Id {userCustomer1.Id} doesn't have any bookings!", ex.Message);
        }

        [Test]
        public async Task CanRatingBeGiven_ShouldFail_WhenTryingToBookBeforeTheCheckoutDateHasPassed()
        {
            await CreateDummyBooking();

            LoggedInAs = booking1.BookedBy;

            var result = typeof(BookingService)
                            .GetMethod("CanRatingBeGiven", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.Throws<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { booking1, booking1.BookedBy, 3 }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(ParameterException));

            Assert.AreEqual("Rating cannot be given until after checking out!", ex.InnerException.Message);
        }

        [Test]
        public async Task CanRatingBeGiven_ShouldFail_WhenRatingGivenBySomeoneElseThanTheBooker()
        {
            await CreateDummyBooking();

            LoggedInAs = userAdmin;

            var result = typeof(BookingService)
                            .GetMethod("CanRatingBeGiven", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.Throws<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { booking1, booking1.BookedBy, 3 }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(AccessException));

            Assert.AreEqual("Only the booker can leave a rating!", ex.InnerException.Message);
        }

        [Test]
        public async Task CanRatingBeGiven_ShouldFail_WhenBookingHasAlreadyBeenRated()
        {
            await CreateDummyBooking();

            LoggedInAs = booking1.BookedBy;

            booking1.Rating = 2;

            var result = typeof(BookingService)
                            .GetMethod("CanRatingBeGiven", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.Throws<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { booking1, booking1.BookedBy, 3 }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(ParameterException));

            Assert.AreEqual("Rating cannot be given twice!", ex.InnerException.Message);
        }

        [Test]
        public async Task CanRatingBeGiven_ShouldFail_IfRatingIsLessThanOneOrHigherThanFive()
        {
            await CreateDummyBooking();

            LoggedInAs = booking1.BookedBy;

            var result = typeof(BookingService)
                            .GetMethod("CanRatingBeGiven", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.Throws<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { booking1, booking1.BookedBy, 0 }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(ParameterException));

            Assert.AreEqual("Rating cannot be less than 1 or bigger than 5!", ex.InnerException.Message);

            result = typeof(BookingService)
                            .GetMethod("CanRatingBeGiven", BindingFlags.NonPublic | BindingFlags.Instance);

            ex = Assert.Throws<TargetInvocationException>(()
                => result.Invoke(bookingService, new object[] { booking1, booking1.BookedBy, 6 }));

            Assert.AreEqual("Rating cannot be less than 1 or bigger than 5!", ex.InnerException.Message);
        }
    }
}
