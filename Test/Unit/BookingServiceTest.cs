using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Imp;
using AintBnB.Core.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace Test.Unit
{
    [TestFixture]
    public class BookingServiceTest : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            SetupDatabaseForTesting();
            SetupTestClasses();
            CreateDummyUsers();
            CreateDummyAccommodation();
        }

        [TearDown]
        public void TearDown()
        {
            Dispose();
        }

        [Test]
        public void Book_ShouldReturn_NewBookingIfDatesAreAvailable()
        {
            LoggedInAs = userCustomer2;

            string startDate = DateTime.Today.ToString("yyyy-MM-dd");
            int nights = 2;

            Booking bk = bookingService.Book(startDate, userCustomer2, nights, accommodation1);

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

            string startDate = DateTime.Today.ToString("yyyy-MM-dd");
            int nights = 2;

            var ex = Assert.Throws<ParameterException>(()
                => bookingService.Book(startDate, userCustomer1, nights, accommodation1));

            Assert.AreEqual("Accommodation cannot be booked by the owner!", ex.Message);
        }

        [Test]
        public void Book_ShouldFail_IfNightsAreLessThanOne()
        {
            LoggedInAs = userCustomer2;

            string startDate = DateTime.Today.ToString("yyyy-MM-dd");
            int nights = 0;

            var ex = Assert.Throws<ParameterException>(()
                => bookingService.Book(startDate, userCustomer2, nights, accommodation1));

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

            string startDate = DateTime.Today.ToString("yyyy-MM-dd");
            int nights = 2;

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

            string startDate = DateTime.Today.ToString("yyyy-MM-dd");
            int nights = 2;

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

            string startDate = DateTime.Today.ToString("yyyy-MM-dd");
            int nights = 2;

            var result = typeof(BookingService)
                            .GetMethod("BookIfAvailableAndUserHasPermission", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.DoesNotThrow(()
                => result.Invoke(bookingService, new object[] { startDate, userCustomer2, nights, accommodation1 }));
        }

        [Test]
        public void BookIfAvailableAndUserHasPermission_ShouldPass_IfEmployeeTriesToBookForACustomer()
        {
            LoggedInAs = userEmployee1;

            string startDate = DateTime.Today.ToString("yyyy-MM-dd");
            int nights = 2;

            var result = typeof(BookingService)
                            .GetMethod("BookIfAvailableAndUserHasPermission", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.DoesNotThrow(()
                => result.Invoke(bookingService, new object[] { startDate, userCustomer2, nights, accommodation1 }));
        }

        [Test]
        public void TryToBookIfAllDatesAvailable_ShouldFail_IfAllDatesAreNotAvailable()
        {
            CreateDummyBooking();

            string startDate = booking1.Dates[booking1.Dates.Count - 1];
            int nights = 2;

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
            string startDate = DateTime.Today.ToString("yyyy-MM-dd");

            int nights = 2;

            var result = typeof(BookingService)
                            .GetMethod("TryToBookIfAllDatesAvailable", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.DoesNotThrow(()
                => result.Invoke(bookingService, new object[] { startDate, userCustomer2, nights, accommodation1 }));
        }

        [Test]
        public void UpdateBooking_ShouldSucceed_IfNewDatesAreNotBookedByOthers_CaseWhenNewStartDateIsBeforeOriginal_AndNewCheckoutDateIsAfterOriginal()
        {
            CreateDummyBooking();

            LoggedInAs = booking1.BookedBy;

            DateTime newStartDateTime = DateTime.Today.AddDays(1);
            DateTime originalStartDateTime = DateTime.Parse(booking1.Dates[0]);
            DateTime originalCheckoutDateTime = DateTime.Parse(booking1.Dates[booking1.Dates.Count - 1]);
            int originalPrice = booking1.Price;
            List<string> orignalDates = booking1.Dates;

            string newStartDate = newStartDateTime.ToString("yyyy-MM-dd");
            int nights = 7;

            bookingService.UpdateBooking(newStartDate, nights, booking1.Id);

            DateTime newCheckOutDateTime = DateTime.Parse(booking1.Dates[booking1.Dates.Count - 1]);
            string newCheckOutDate = newCheckOutDateTime.ToString("yyyy-MM-dd");

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
        public void UpdateBooking_Should_SetUpdatedDatesToUnavailableInSchedule()
        {
            CreateDummyBooking();

            LoggedInAs = booking1.BookedBy;

            DateTime newStartDateTime = DateTime.Today.AddDays(1);
            DateTime originalStartDateTime = DateTime.Parse(booking1.Dates[0]);
            DateTime originalCheckoutDateTime = DateTime.Parse(booking1.Dates[booking1.Dates.Count - 1]);
            int originalPrice = booking1.Price;
            List<string> orignalDates = booking1.Dates;

            string newStartDate = newStartDateTime.ToString("yyyy-MM-dd");
            int nights = 7;

            bookingService.UpdateBooking(newStartDate, nights, booking1.Id);

            DateTime newCheckOutDateTime = DateTime.Parse(booking1.Dates[booking1.Dates.Count - 1]);
            string newCheckOutDate = newCheckOutDateTime.ToString("yyyy-MM-dd");


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
        public void UpdateBooking_ShouldSucceed_AndSetTheOriginalDatesThatAreNotBookedAnyLongerToAvailable_CaseWhenNewCheckOutDateIsBeforeOriginal()
        {
            CreateDummyBooking();

            LoggedInAs = booking1.BookedBy;

            DateTime newStartDateTime = DateTime.Today.AddDays(1);
            DateTime originalStartDateTime = DateTime.Parse(booking1.Dates[0]);
            DateTime originalCheckoutDateTime = DateTime.Parse(booking1.Dates[booking1.Dates.Count - 1]);
            List<string> orignalDates = booking1.Dates;

            string newStartDate = newStartDateTime.ToString("yyyy-MM-dd");
            int nights = 5;

            bookingService.UpdateBooking(newStartDate, nights, booking1.Id);

            DateTime newCheckOutDateTime = DateTime.Parse(booking1.Dates[booking1.Dates.Count - 1]);
            string newCheckOutDate = newCheckOutDateTime.ToString("yyyy-MM-dd");

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
        public void UpdateBooking_ShouldSucceed_AndSetTheOriginalDatesThatAreNotBookedAnyLongerToAvailable_CaseWhenNewStartDateIsAfterOriginal()
        {
            CreateDummyBooking();

            LoggedInAs = booking1.BookedBy;

            DateTime newStartDateTime = DateTime.Today.AddDays(3);
            DateTime originalStartDateTime = DateTime.Parse(booking1.Dates[0]);
            DateTime originalCheckoutDateTime = DateTime.Parse(booking1.Dates[booking1.Dates.Count - 1]);
            List<string> orignalDates = booking1.Dates;

            string newStartDate = newStartDateTime.ToString("yyyy-MM-dd");
            int nights = 4;

            bookingService.UpdateBooking(newStartDate, nights, booking1.Id);

            DateTime newCheckOutDateTime = DateTime.Parse(booking1.Dates[booking1.Dates.Count - 1]);
            string newCheckOutDate = newCheckOutDateTime.ToString("yyyy-MM-dd");

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
        public void UpdateBooking_ShouldSucceed_AndUpdateOldDatesToAvailableAndNewDatesToUnavailable_CaseWhenAllTheNewDatesAreDifferentThanOriginal()
        {
            CreateDummyBooking();

            LoggedInAs = booking1.BookedBy;

            DateTime newStartDateTime = DateTime.Today.AddDays(15);
            List<string> orignalDates = booking1.Dates;

            string newStartDate = newStartDateTime.ToString("yyyy-MM-dd");
            int nights = 5;

            bookingService.UpdateBooking(newStartDate, nights, booking1.Id);


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
        public void GetBooking_ShouldReturn_CorrectBooking()
        {
            CreateDummyBooking();

            LoggedInAs = userCustomer2;

            Assert.AreEqual(booking1.Id, bookingService.GetBooking(1).Id);
        }

        [Test]
        public void GetBooking_ShouldReturn_CorrectBookingIfOwnerOfAccommodationTriesToGetABookingOfTheirAccommodationByAnotherUser()
        {
            CreateDummyBooking();

            LoggedInAs = userCustomer1;

            Assert.AreEqual(booking1.Id, bookingService.GetBooking(1).Id);
        }


        [Test]
        public void GetBooking_ShouldFail_IfIdDoesNotExist()
        {
            CreateDummyBooking();

            LoggedInAs = userCustomer1;

            var ex = Assert.Throws<IdNotFoundException>(()
                => bookingService.GetBooking(100));

            Assert.AreEqual("Booking with ID 100 not found!", ex.Message);
        }

        [Test]
        public void GetBooking_ShouldFail_IfUserThatIsNotOwnerOfAccommodationOrAdminOrEmployeeTriesToGetBookingOfAnotherUser()
        {
            CreateDummyBooking();

            LoggedInAs = userRequestToBecomeEmployee;

            var ex = Assert.Throws<AccessException>(()
                => bookingService.GetBooking(1));

            Assert.AreEqual("Restricted access!", ex.Message);
        }

        [Test]
        public void GetBookingsOfOwnedAccommodation_ShouldReturn_ListOfAllBookingsOnTheAccommodationOfTheUser()
        {
            CreateDummyBooking();

            LoggedInAs = userCustomer2;

            Assert.AreEqual(2, bookingService.GetBookingsOfOwnedAccommodation(6).Count);
            Assert.AreEqual(booking2.Id, bookingService.GetBookingsOfOwnedAccommodation(6)[0].Id);
            Assert.AreEqual(booking3.Id, bookingService.GetBookingsOfOwnedAccommodation(6)[1].Id);
            Assert.AreEqual(booking2.Accommodation.Owner, userCustomer2);
            Assert.AreEqual(booking3.Accommodation.Owner, userCustomer2);
        }

        [Test]
        public void GetBookingsOfOwnedAccommodation_ShouldFail_IfThereAreNoBookingsOnTheAccommodationsOfTheUser()
        {
            CreateDummyBooking();

            LoggedInAs = userRequestToBecomeEmployee;

            userRequestToBecomeEmployee.UserType = UserTypes.Customer;

            adr.Id = 100;

            accommodationService.CreateAccommodation(userRequestToBecomeEmployee, adr, 1, 2, 1, "d", 1, 1, new List<byte[]>(), 10);

            var ex = Assert.Throws<NoneFoundInDatabaseTableException>(()
                => bookingService.GetBookingsOfOwnedAccommodation(3));

            Assert.AreEqual($"User with Id {userRequestToBecomeEmployee.Id} doesn't have any bookings of owned accommodations!", ex.Message);
        }

        [Test]
        public void GetAllBookings_ShouldReturn_AllBookingsInTheSystemIfAdmin()
        {
            CreateDummyBooking();

            LoggedInAs = userAdmin;

            List<Booking> allBookings = bookingService.GetAllBookings();

            Assert.AreEqual(3, allBookings.Count);
            Assert.True(allBookings.Contains(booking1));
            Assert.True(allBookings.Contains(booking2));
            Assert.True(allBookings.Contains(booking3));
        }

        [Test]
        public void GetAllBookings_ShouldReturn_AllBookingsInTheSystemIfEmployee()
        {
            CreateDummyBooking();

            LoggedInAs = userEmployee1;

            List<Booking> allBookings = bookingService.GetAllBookings();

            Assert.AreEqual(3, allBookings.Count);
            Assert.True(allBookings.Contains(booking1));
            Assert.True(allBookings.Contains(booking2));
            Assert.True(allBookings.Contains(booking3));
        }

        [Test]
        public void GetAllBookings_ShouldReturn_AllBookingsOfTheCustomerIfNormalCustomer()
        {
            CreateDummyBooking();

            LoggedInAs = userCustomer1;

            List<Booking> allBookings = bookingService.GetAllBookings();

            Assert.AreEqual(2, allBookings.Count);
            Assert.True(allBookings.Contains(booking2));
            Assert.True(allBookings.Contains(booking3));
        }

        [Test]
        public void GetAllBookings_ShouldFail_IfThereAreNoBookingsInTheSystem()
        {
            LoggedInAs = userAdmin;

            var ex = Assert.Throws<NoneFoundInDatabaseTableException>(()
                => bookingService.GetAllBookings());

            Assert.AreEqual("No bookings found!", ex.Message);
        }

        [Test]
        public void GetAllBookings_ShouldFail_IfCustomerDoesNotHaveAnyBookings()
        {
            LoggedInAs = userCustomer1;

            var ex = Assert.Throws<NoneFoundInDatabaseTableException>(()
                => bookingService.GetAllBookings());

            Assert.AreEqual($"User with Id {userCustomer1.Id} doesn't have any bookings!", ex.Message);
        }
    }
}
