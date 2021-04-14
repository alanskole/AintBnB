using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Imp;
using NUnit.Framework;
using System.Reflection;
using System.Threading.Tasks;

using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace Test.Unit
{
    [TestFixture]
    public class DeletionServiceTest : TestBase
    {
        [SetUp]
        public async Task SetUp()
        {
            await SetupDatabaseForTesting();
            SetupTestClasses();
            await CreateDummyUsers();
            await CreateDummyAccommodation();
            await CreateDummyBooking();
        }

        [TearDown]
        public async Task TearDown()
        {
            await DisposeAsync();
        }

        [Test]
        public async Task DeleteUser_ShouldFail_IfTheUserHasABookingOfTheirAccommodationThatCannotBeDeletedBecauseOfCancellationDeadlineExpired()
        {
            LoggedInAs = null;

            var all = await userService.GetAllUsersForLoginAsync();

            Assert.True(all.Contains(userCustomer1));

            LoggedInAs = booking1.BookedBy;

            var ex = Assert.ThrowsAsync<CancelBookingException>(async ()
                => await deletionService.DeleteUserAsync(booking1.BookedBy.Id));

            Assert.AreEqual($"The accommodation cannot be deleted because it has a booking with ID {booking3.Id} with a start date less than {booking3.Accommodation.CancellationDeadlineInDays} days away! DeleteAsync when no bookings are less than {booking3.Accommodation.CancellationDeadlineInDays} days away.", ex.Message);
        }

        [Test]
        public async Task DeleteUser_ShouldFail_IfTheUserHasABookingThatCannotBeDeletedBecauseOfCancellationDeadlineExpired()
        {
            LoggedInAs = null;

            var all = await userService.GetAllUsersForLoginAsync();

            Assert.True(all.Contains(userCustomer1));

            LoggedInAs = booking2.BookedBy;

            var ex = Assert.ThrowsAsync<CancelBookingException>(async()
                => await deletionService.DeleteUserAsync(booking2.BookedBy.Id));

            Assert.AreEqual($"The user cannot be deleted because it has a booking with ID {booking3.Id} with a start date less than {booking3.Accommodation.CancellationDeadlineInDays} days away! DeleteAsync when no bookings are less than {booking3.Accommodation.CancellationDeadlineInDays} days away.", ex.Message);
        }

        [Test]
        public async Task DeleteUser_ShouldSucceed_IfNoCancellationDeadlineExpiredOnBookingsOrAccommodations()
        {
            LoggedInAs = null;

            var all = await userService.GetAllUsersForLoginAsync();

            Assert.True(all.Contains(userCustomer1));

            LoggedInAs = booking2.BookedBy;

            booking3.Accommodation.CancellationDeadlineInDays = 1;

            await deletionService.DeleteUserAsync(booking2.BookedBy.Id);

            all = await userService.GetAllUsersForLoginAsync();

            Assert.False(all.Contains(userCustomer1));
        }

        [Test]
        public async Task DeleteUser_ShouldSucceed_IfAdminDeletesACustomer()
        {
            LoggedInAs = userAdmin;

            var all = await userService.GetAllUsersAsync();

            Assert.True(all.Contains(userCustomer1));

            booking3.Accommodation.CancellationDeadlineInDays = 1;

            await deletionService.DeleteUserAsync(booking2.BookedBy.Id);

            all = await userService.GetAllUsersAsync();
            
            Assert.False(all.Contains(userCustomer1));
        }

        [Test]
        public void DeleteUser_ShouldFail_IfEmployeeTriesToDeleteAccount()
        {
            LoggedInAs = userEmployee1;

            var ex = Assert.ThrowsAsync<AccessException>(async()
                => await deletionService.DeleteUserAsync(userEmployee1.Id));

            Assert.AreEqual("Employees cannot delete any accounts, even if it's their own accounts!", ex.Message);
        }

        [Test]
        public void DeleteUser_ShouldFail_IfCustomerTriesToDeleteAccountNotBelongingToThem()
        {
            LoggedInAs = userCustomer1;

            var ex = Assert.ThrowsAsync<AccessException>(async()
                => await deletionService.DeleteUserAsync(userCustomer2.Id));

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
        public async Task DeleteUsersAccommodations_ShouldSucceed_AndDeleteAllAccommodationsOfTheDeletedUserIfNotBlockedByCancellationDeadline()
        {
            LoggedInAs = userAdmin;

            var all = await accommodationService.GetAllAccommodationsAsync();

            Assert.True(all.Contains(accommodation1));

            booking3.Accommodation.CancellationDeadlineInDays = 1;

            var result = typeof(DeletionService)
                .GetMethod("DeleteUsersAccommodationsAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            result.Invoke(deletionService, new object[] { booking3.BookedBy.Id });

            all = await accommodationService.GetAllAccommodationsAsync();

            Assert.False(all.Contains(accommodation1));
        }

        [Test]
        public async Task DeleteUsersBookings_ShouldSucceed_AndDeleteAllBookingsOfTheDeletedUserIfNotBlockedByCancellationDeadline()
        {
            LoggedInAs = userAdmin;

            var all = await bookingService.GetAllBookingsAsync();

            Assert.True(all.Contains(booking2));
            Assert.True(all.Contains(booking3));

            booking3.Accommodation.CancellationDeadlineInDays = 1;

            var result = typeof(DeletionService)
                .GetMethod("DeleteUsersBookingsAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            result.Invoke(deletionService, new object[] { booking3.BookedBy.Id });

            all = await bookingService.GetAllBookingsAsync();

            Assert.False(all.Contains(booking2));
            Assert.False(all.Contains(booking3));
        }

        [Test]
        public async Task DeleteAccommodation_ShouldSucceed_IfNotBlockedByCancellationDeadlineExpiration()
        {
            LoggedInAs = accommodation2.Owner;

            var all = await accommodationService.GetAllAccommodationsAsync();

            Assert.True(all.Contains(accommodation2));

            await deletionService.DeleteAccommodationAsync(accommodation2.Id);

            all = await accommodationService.GetAllAccommodationsAsync();

            Assert.False(all.Contains(accommodation2));
        }

        [Test]
        public async Task DeleteAccommodationBookings_ShouldSucceed_IfNotBlockedByCancellationDeadlineExpiration()
        {
            LoggedInAs = accommodation2.Owner;

            var all = await bookingService.GetBookingsOfOwnedAccommodationAsync(accommodation2.Owner.Id);

            Assert.True(all.Contains(booking2));

            var result = typeof(DeletionService)
                .GetMethod("DeleteAccommodationBookingsAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            result.Invoke(deletionService, new object[] { accommodation2.Id });

            all = await bookingService.GetBookingsOfOwnedAccommodationAsync(accommodation2.Owner.Id);

            Assert.False(all.Contains(booking2));
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
        public async Task DeleteBooking_ShouldSucceed_IfNotBlockedByCancellationDeadlineExpiration()
        {
            LoggedInAs = booking2.BookedBy;

            var all = await bookingService.GetAllBookingsAsync();

            Assert.True(all.Contains(booking2));

            await deletionService.DeleteBookingAsync(booking2.Id);

            all = await bookingService.GetAllBookingsAsync();

            Assert.False(all.Contains(booking2));
        }

        [Test]
        public void DeadLineExpiration_ShouldSucceed_IfCancellationDeadlineHasNotExpired()
        {
            var result = typeof(DeletionService)
                .GetMethod("DeadLineExpirationAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.DoesNotThrow(() => result.Invoke(deletionService, new object[] { booking2.Id, booking2.Accommodation.CancellationDeadlineInDays }));
        }

        [Test]
        public void DeadLineExpiration_ShouldFail_IfCancellationDeadlineHasNotExpired()
        {
            var result = typeof(DeletionService)
                .GetMethod("DeadLineExpirationAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.ThrowsAsync<CancelBookingException>(async()
                => await (Task)result.Invoke(deletionService, new object[] { booking3.Id, booking3.Accommodation.CancellationDeadlineInDays }));

            Assert.AreEqual($"Cannot change the booking with ID {booking3.Id} because the start date of the booking is less than {booking3.Accommodation.CancellationDeadlineInDays} days away!", ex.Message);
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
                .GetMethod("ResetAvailableStatusAfterDeletingBookingAsync", BindingFlags.NonPublic | BindingFlags.Instance);
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
