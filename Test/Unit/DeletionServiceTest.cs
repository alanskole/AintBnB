using AintBnB.Core.Models;
using NUnit.Framework;
using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Imp;
using System.Reflection;
using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace Test.Unit
{
    [TestFixture]
    public class DeletionServiceTest : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            SetupDatabaseForTesting();
            SetupTestClasses();
            CreateDummyUsers();
            CreateDummyAccommodation();
            CreateDummyBooking();
        }

        [TearDown]
        public void TearDown()
        {
            Dispose();
        }

        [Test]
        public void DeleteUser_ShouldFail_IfTheUserHasABookingOfTheirAccommodationThatCannotBeDeletedBecauseOfCancellationDeadlineExpired()
        {
            LoggedInAs = null;

            Assert.True(userService.GetAllUsersForLogin().Contains(userCustomer1));

            LoggedInAs = booking1.BookedBy;

            var ex = Assert.Throws<CancelBookingException>(()
                => deletionService.DeleteUser(booking1.BookedBy.Id));

            Assert.AreEqual($"The accommodation cannot be deleted because it has a booking with ID {booking3.Id} with a start date less than {booking3.Accommodation.CancellationDeadlineInDays} days away! Delete when no bookings are less than {booking3.Accommodation.CancellationDeadlineInDays} days away.", ex.Message);
        }

        [Test]
        public void DeleteUser_ShouldFail_IfTheUserHasABookingThatCannotBeDeletedBecauseOfCancellationDeadlineExpired()
        {
            LoggedInAs = null;

            Assert.True(userService.GetAllUsersForLogin().Contains(userCustomer1));

            LoggedInAs = booking2.BookedBy;

            var ex = Assert.Throws<CancelBookingException>(()
                => deletionService.DeleteUser(booking2.BookedBy.Id));

            Assert.AreEqual($"The user cannot be deleted because it has a booking with ID {booking3.Id} with a start date less than {booking3.Accommodation.CancellationDeadlineInDays} days away! Delete when no bookings are less than {booking3.Accommodation.CancellationDeadlineInDays} days away.", ex.Message);
        }

        [Test]
        public void DeleteUser_ShouldSucceed_IfNoCancellationDeadlineExpiredOnBookingsOrAccommodations()
        {
            LoggedInAs = null;

            Assert.True(userService.GetAllUsersForLogin().Contains(userCustomer1));

            LoggedInAs = booking2.BookedBy;

            booking3.Accommodation.CancellationDeadlineInDays = 1;

            deletionService.DeleteUser(booking2.BookedBy.Id);

            Assert.False(userService.GetAllUsersForLogin().Contains(userCustomer1));
        }

        [Test]
        public void DeleteUser_ShouldSucceed_IfAdminDeletesACustomer()
        {
            LoggedInAs = userAdmin;

            Assert.True(userService.GetAllUsers().Contains(userCustomer1));

            booking3.Accommodation.CancellationDeadlineInDays = 1;

            deletionService.DeleteUser(booking2.BookedBy.Id);

            Assert.False(userService.GetAllUsers().Contains(userCustomer1));
        }

        [Test]
        public void DeleteUser_ShouldFail_IfEmployeeTriesToDeleteAccount()
        {
            LoggedInAs = userEmployee1;

            var ex = Assert.Throws<AccessException>(()
                => deletionService.DeleteUser(userEmployee1.Id));

            Assert.AreEqual("Employees cannot delete any accounts, even if it's their own accounts!", ex.Message);
        }

        [Test]
        public void DeleteUser_ShouldFail_IfCustomerTriesToDeleteAccountNotBelongingToThem()
        {
            LoggedInAs = userCustomer1;

            var ex = Assert.Throws<AccessException>(()
                => deletionService.DeleteUser(userCustomer2.Id));

            Assert.AreEqual($"Administrator or user with ID {userCustomer2.Id} only!", ex.Message);
        }

        [Test]
        public void CheckIfUserCanBeDeleted_ShouldFail_IfAdminAccountTriesToGetDeleted()
        {
            LoggedInAs = userAdmin;

            var result = typeof(DeletionService)
                            .GetMethod("CheckIfUserCanBeDeleted", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.Throws<TargetInvocationException>(()
                => result.Invoke(deletionService, new object[] { userAdmin }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(AccessException));

            Assert.AreEqual("Admin cannot be deleted!", ex.InnerException.Message);
        }

        [Test]
        public void DeleteUsersAccommodations_ShouldSucceed_AndDeleteAllAccommodationsOfTheDeletedUserIfNotBlockedByCancellationDeadline()
        {
            LoggedInAs = userAdmin;

            Assert.True(accommodationService.GetAllAccommodations().Contains(accommodation1));

            booking3.Accommodation.CancellationDeadlineInDays = 1;

            var result = typeof(DeletionService)
                .GetMethod("DeleteUsersAccommodations", BindingFlags.NonPublic | BindingFlags.Instance);

            result.Invoke(deletionService, new object[] { booking3.BookedBy.Id });

            Assert.False(accommodationService.GetAllAccommodations().Contains(accommodation1));
        }

        [Test]
        public void DeleteUsersBookings_ShouldSucceed_AndDeleteAllBookingsOfTheDeletedUserIfNotBlockedByCancellationDeadline()
        {
            LoggedInAs = userAdmin;

            Assert.True(bookingService.GetAllBookings().Contains(booking2));
            Assert.True(bookingService.GetAllBookings().Contains(booking3));

            booking3.Accommodation.CancellationDeadlineInDays = 1;

            var result = typeof(DeletionService)
                .GetMethod("DeleteUsersBookings", BindingFlags.NonPublic | BindingFlags.Instance);

            result.Invoke(deletionService, new object[] { booking3.BookedBy.Id });

            Assert.False(bookingService.GetAllBookings().Contains(booking2));
            Assert.False(bookingService.GetAllBookings().Contains(booking3));
        }

        [Test]
        public void DeleteAccommodation_ShouldSucceed_IfNotBlockedByCancellationDeadlineExpiration()
        {
            LoggedInAs = accommodation2.Owner;

            Assert.True(accommodationService.GetAllAccommodations().Contains(accommodation2));

            deletionService.DeleteAccommodation(accommodation2.Id);

            Assert.False(accommodationService.GetAllAccommodations().Contains(accommodation2));
        }

        [Test]
        public void DeleteAccommodationBookings_ShouldSucceed_IfNotBlockedByCancellationDeadlineExpiration()
        {
            LoggedInAs = accommodation2.Owner;

            Assert.True(bookingService.GetBookingsOfOwnedAccommodation(accommodation2.Owner.Id).Contains(booking2));

            var result = typeof(DeletionService)
                .GetMethod("DeleteAccommodationBookings", BindingFlags.NonPublic | BindingFlags.Instance);

            result.Invoke(deletionService, new object[] { accommodation2.Id });

            Assert.False(bookingService.GetBookingsOfOwnedAccommodation(accommodation2.Owner.Id).Contains(booking2));
        }


        [Test]
        public void CanAccommodationBeDeleted_ShouldSucceed_IfAdmin()
        {
            LoggedInAs = userAdmin;

            var result = typeof(DeletionService)
                .GetMethod("CanAccommodationBeDeleted", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.DoesNotThrow(() => result.Invoke(deletionService, new object[] { accommodation2 }));
        }

        [Test]
        public void CanAccommodationBeDeleted_ShouldSucceed_IfEmployee()
        {
            LoggedInAs = userEmployee1;

            var result = typeof(DeletionService)
                .GetMethod("CanAccommodationBeDeleted", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.DoesNotThrow(() => result.Invoke(deletionService, new object[] { accommodation2 }));
        }

        [Test]
        public void CanAccommodationBeDeleted_ShouldSucceed_IfCustomerDeletingTheirOwnAccommodation()
        {
            LoggedInAs = userCustomer2;

            var result = typeof(DeletionService)
                .GetMethod("CanAccommodationBeDeleted", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.DoesNotThrow(() => result.Invoke(deletionService, new object[] { accommodation2 }));
        }

        [Test]
        public void CanAccommodationBeDeleted_ShouldFail_IfCustomerTriesToDeleteAccommodationOfOtherCustomer()
        {
            LoggedInAs = userCustomer1;

            var result = typeof(DeletionService)
                .GetMethod("CanAccommodationBeDeleted", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.Throws<TargetInvocationException>(()
                => result.Invoke(deletionService, new object[] { accommodation2 }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(AccessException));

            Assert.AreEqual($"Administrator, employee or user with ID {accommodation2.Owner.Id} only!", ex.InnerException.Message);
        }

        [Test]
        public void DeleteBooking_ShouldSucceed_IfNotBlockedByCancellationDeadlineExpiration()
        {
            LoggedInAs = booking2.BookedBy;

            Assert.True(bookingService.GetAllBookings().Contains(booking2));

            deletionService.DeleteBooking(booking2.Id);

            Assert.False(bookingService.GetAllBookings().Contains(booking2));
        }

        [Test]
        public void DeadLineExpiration_ShouldSucceed_IfCancellationDeadlineHasNotExpired()
        {
            var result = typeof(DeletionService)
                .GetMethod("DeadLineExpiration", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.DoesNotThrow(() => result.Invoke(deletionService, new object[] { booking2.Id,  booking2.Accommodation.CancellationDeadlineInDays }));
        }

        [Test]
        public void DeadLineExpiration_ShouldFail_IfCancellationDeadlineHasNotExpired()
        {
            var result = typeof(DeletionService)
                .GetMethod("DeadLineExpiration", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.Throws<TargetInvocationException>(()
                            => result.Invoke(deletionService, new object[] { booking3.Id, booking3.Accommodation.CancellationDeadlineInDays }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(CancelBookingException));

            Assert.AreEqual($"Cannot change the booking with ID {booking3.Id} because the start date of the booking is less than {booking3.Accommodation.CancellationDeadlineInDays} days away!", ex.InnerException.Message);
        }

        [Test]
        public void ResetAvailableStatusAfterDeletingBooking_ShouldSucceed_IfBookingDatesCancelled()
        {
            LoggedInAs = booking2.BookedBy;

            Assert.False(booking2.Accommodation.Schedule[booking2.Dates[0]]);
            Assert.False(booking2.Accommodation.Schedule[booking2.Dates[1]]);
            Assert.False(booking2.Accommodation.Schedule[booking2.Dates[2]]);
            Assert.False(booking2.Accommodation.Schedule[booking2.Dates[3]]);
            Assert.False(booking2.Accommodation.Schedule[booking2.Dates[4]]);
            Assert.False(booking2.Accommodation.Schedule[booking2.Dates[5]]);
            Assert.False(booking2.Accommodation.Schedule[booking2.Dates[6]]);
            Assert.False(booking2.Accommodation.Schedule[booking2.Dates[7]]);
            Assert.False(booking2.Accommodation.Schedule[booking2.Dates[8]]);
            Assert.False(booking2.Accommodation.Schedule[booking2.Dates[9]]);
            Assert.False(booking2.Accommodation.Schedule[booking2.Dates[10]]);
            Assert.False(booking2.Accommodation.Schedule[booking2.Dates[11]]);

            var result = typeof(DeletionService)
                .GetMethod("ResetAvailableStatusAfterDeletingBooking", BindingFlags.NonPublic | BindingFlags.Instance);
            result.Invoke(deletionService, new object[] { booking2.Id });

            Assert.True(booking2.Accommodation.Schedule[booking2.Dates[0]]);
            Assert.True(booking2.Accommodation.Schedule[booking2.Dates[1]]);
            Assert.True(booking2.Accommodation.Schedule[booking2.Dates[2]]);
            Assert.True(booking2.Accommodation.Schedule[booking2.Dates[3]]);
            Assert.True(booking2.Accommodation.Schedule[booking2.Dates[4]]);
            Assert.True(booking2.Accommodation.Schedule[booking2.Dates[5]]);
            Assert.True(booking2.Accommodation.Schedule[booking2.Dates[6]]);
            Assert.True(booking2.Accommodation.Schedule[booking2.Dates[7]]);
            Assert.True(booking2.Accommodation.Schedule[booking2.Dates[8]]);
            Assert.True(booking2.Accommodation.Schedule[booking2.Dates[9]]);
            Assert.True(booking2.Accommodation.Schedule[booking2.Dates[10]]);
            Assert.True(booking2.Accommodation.Schedule[booking2.Dates[11]]);
        }
    }
}
