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
            await SetupDatabaseForTestingAsync();
            SetupTestClasses();
            await CreateDummyUsersAsync();
            await CreateDummyAccommodationAsync();
            await CreateDummyBookingAsync();
        }

        [TearDown]
        public async Task TearDown()
        {
            await DisposeAsync();
        }

        [Test]
        public async Task DeleteUserAsync_ShouldFail_IfTheUserHasABookingOfTheirAccommodationThatCannotBeDeletedBecauseOfCancellationDeadlineExpired()
        {
            LoggedInAs = booking1.BookedBy;

            var all = await bookingService.GetBookingsOfOwnedAccommodationAsync(booking1.BookedBy.Id);

            Assert.AreEqual(2, all.Count);

            var ex = Assert.ThrowsAsync<CancelBookingException>(async ()
                => await deletionService.DeleteUserAsync(booking1.BookedBy.Id));

            Assert.AreEqual($"The user with ID {booking1.BookedBy.Id} cannot be deleted because it has an accommodation with ID {booking3.Accommodation.Id} that has bookings that can't be deleted because of the cancellation deadline of the accommodation. Delete when no bookings of the accommodation have surpassed the cancellation deadline of {booking3.Accommodation.CancellationDeadlineInDays} days.", ex.Message);

            all = await bookingService.GetBookingsOfOwnedAccommodationAsync(booking1.BookedBy.Id);

            Assert.AreEqual(2, all.Count);
        }

        [Test]
        public void DeleteUserAsync_ShouldFail_IfTheUserHasABookingThatCannotBeDeletedBecauseOfCancellationDeadlineExpired()
        {
            LoggedInAs = booking3.BookedBy;

            var ex = Assert.ThrowsAsync<CancelBookingException>(async ()
                => await deletionService.DeleteUserAsync(booking3.BookedBy.Id));

            Assert.AreEqual($"The user cannot be deleted because it has a booking with ID {booking3.Id} with a start date less than {booking3.Accommodation.CancellationDeadlineInDays} days away! Delete when no bookings are less than {booking3.Accommodation.CancellationDeadlineInDays} days away.", ex.Message);
        }

        [Test]
        public async Task DeleteUserAsync_ShouldDeleteNothing_IfUserCannotBeDeleted()
        {
            LoggedInAs = userAdmin;

            var allUsr = await userService.GetAllUsersAsync();
            var allAcc = await accommodationService.GetAllAccommodationsAsync();
            var allBk = await bookingService.GetAllBookingsAsync();

            Assert.AreEqual(7, allUsr.Count);
            Assert.AreEqual(4, allAcc.Count);
            Assert.AreEqual(6, allBk.Count);

            var ex = Assert.ThrowsAsync<CancelBookingException>(async ()
                => await deletionService.DeleteUserAsync(userCustomer1.Id));

            Assert.AreEqual($"The user cannot be deleted because it has a booking with ID {booking3.Id} with a start date less than {booking3.Accommodation.CancellationDeadlineInDays} days away! Delete when no bookings are less than {booking3.Accommodation.CancellationDeadlineInDays} days away.", ex.Message);

            allAcc = await accommodationService.GetAllAccommodationsAsync();
            allBk = await bookingService.GetAllBookingsAsync();

            Assert.AreEqual(7, allUsr.Count);
            Assert.AreEqual(4, allAcc.Count);
            Assert.AreEqual(6, allBk.Count);
        }

        [Test]
        public async Task DeleteUserAsync_ShouldSucceed_IfNoCancellationDeadlineExpiredOnBookingsOrAccommodations()
        {
            LoggedInAs = null;

            var allUsr = await userService.GetAllUsersForLoginAsync();

            LoggedInAs = userCustomer2;

            Assert.True(allUsr.Contains(userCustomer2));

            booking3.Accommodation.CancellationDeadlineInDays = 1;

            await deletionService.DeleteUserAsync(userCustomer2.Id);

            allUsr = await userService.GetAllUsersForLoginAsync();

            Assert.False(allUsr.Contains(userCustomer2));
        }

        [Test]
        public async Task DeleteUserAsync_ShouldSucceed_IfTheCheckoutDateOfTheBookingOfTheUserToBeDeletedIsInThePast()
        {
            LoggedInAs = userAdmin;

            var all = await userService.GetAllUsersAsync();

            Assert.True(all.Contains(userCustomer3));

            await deletionService.DeleteUserAsync(booking4.BookedBy.Id);

            all = await userService.GetAllUsersAsync();

            Assert.False(all.Contains(userCustomer3));
        }

        [Test]
        public async Task DeleteUserAsync_ShouldSucceed_IfAdminDeletesACustomer()
        {
            LoggedInAs = userAdmin;

            var all = await userService.GetAllUsersAsync();

            Assert.True(all.Contains(userCustomer1));

            booking3.Accommodation.CancellationDeadlineInDays = 1;

            await deletionService.DeleteUserAsync(booking3.BookedBy.Id);

            all = await userService.GetAllUsersAsync();

            Assert.False(all.Contains(userCustomer1));
        }

        [Test]
        public async Task DeleteUserAsync_ShouldDeleteTheAccommodationsBookingsAndBookingsOfAccommodations_OfTheDeletedUser()
        {
            LoggedInAs = userAdmin;

            var allUsr = await userService.GetAllUsersAsync();
            var allAcc = await accommodationService.GetAllAccommodationsAsync();
            var allBk = await bookingService.GetAllBookingsAsync();

            var countOfUsr = allUsr.Count;
            var countOfAcc = allAcc.Count;
            var countOfBk = allBk.Count;

            Assert.True(allUsr.Contains(userCustomer3));
            Assert.True(allAcc.Contains(accommodation4));
            Assert.True((accommodation4.Owner.Id == userCustomer3.Id));
            Assert.True(allBk.Contains(booking4));
            Assert.True(allBk.Contains(booking5));
            Assert.True(allBk.Contains(booking6));
            Assert.True((booking4.BookedBy.Id == userCustomer3.Id));
            Assert.True((booking5.Accommodation.Owner.Id == userCustomer3.Id));
            Assert.True((booking6.BookedBy.Id == userCustomer3.Id));

            await deletionService.DeleteUserAsync(userCustomer3.Id);

            allUsr = await userService.GetAllUsersAsync();
            allAcc = await accommodationService.GetAllAccommodationsAsync();
            allBk = await bookingService.GetAllBookingsAsync();

            Assert.False(allUsr.Contains(userCustomer3));
            Assert.False(allAcc.Contains(accommodation4));
            Assert.False(allBk.Contains(booking4));
            Assert.False(allBk.Contains(booking5));
            Assert.False(allBk.Contains(booking6));
            Assert.AreEqual(countOfUsr - 1, allUsr.Count);
            Assert.AreEqual(countOfAcc - 1, allAcc.Count);
            Assert.AreEqual(countOfBk - 3, allBk.Count);
        }

        [Test]
        public void DeleteUserAsync_ShouldFail_IfEmployeeTriesToDeleteAccount()
        {
            LoggedInAs = userEmployee1;

            var ex = Assert.ThrowsAsync<AccessException>(async ()
                => await deletionService.DeleteUserAsync(userEmployee1.Id));

            Assert.AreEqual("Employees cannot delete any accounts, even if it's their own accounts!", ex.Message);
        }

        [Test]
        public void DeleteUserAsync_ShouldFail_IfCustomerTriesToDeleteAccountNotBelongingToThem()
        {
            LoggedInAs = userCustomer1;

            var ex = Assert.ThrowsAsync<AccessException>(async ()
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
        public void DeleteUsersAccommodationsAsync_ShouldSucceed_IfNotBlockedByCancellationDeadline()
        {
            LoggedInAs = userAdmin;

            booking3.Accommodation.CancellationDeadlineInDays = 1;

            var result = typeof(DeletionService)
                .GetMethod("DeleteUsersAccommodationsAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.DoesNotThrowAsync(async () => await (Task)result.Invoke(deletionService, new object[] { booking3.Accommodation.Owner.Id }));
        }

        [Test]
        public void DeleteUsersAccommodationsAsync_ShouldFail_IfBlockedByCancellationDeadline()
        {
            LoggedInAs = userAdmin;

            var result = typeof(DeletionService)
                .GetMethod("DeleteUsersAccommodationsAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.ThrowsAsync<CancelBookingException>(async ()
                => await (Task)result.Invoke(deletionService, new object[] { booking3.Accommodation.Owner.Id }));

            Assert.AreEqual($"The user with ID {booking1.BookedBy.Id} cannot be deleted because it has an accommodation with ID {booking3.Accommodation.Id} that has bookings that can't be deleted because of the cancellation deadline of the accommodation. Delete when no bookings of the accommodation have surpassed the cancellation deadline of {booking3.Accommodation.CancellationDeadlineInDays} days.", ex.Message);
        }

        [Test]
        public void DeleteUsersBookingsAsync_ShouldSucceed_IfNotBlockedByCancellationDeadline()
        {
            LoggedInAs = userAdmin;

            booking3.Accommodation.CancellationDeadlineInDays = 1;

            var result = typeof(DeletionService)
                .GetMethod("DeleteUsersBookingsAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.DoesNotThrowAsync(async () => await (Task)result.Invoke(deletionService, new object[] { booking3.BookedBy.Id }));
        }

        [Test]
        public void DeleteUsersBookingsAsync_ShouldFail_IfBlockedByCancellationDeadline()
        {
            LoggedInAs = userAdmin;

            var result = typeof(DeletionService)
                .GetMethod("DeleteUsersBookingsAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.ThrowsAsync<CancelBookingException>(async ()
                => await (Task)result.Invoke(deletionService, new object[] { booking3.BookedBy.Id }));

            Assert.AreEqual("The user cannot be deleted because it has a booking with ID 3 with a start date less than 3 days away! Delete when no bookings are less than 3 days away.", ex.Message);
        }

        [Test]
        public async Task DeleteAccommodationAsync_ShouldSucceed_IfNotBlockedByCancellationDeadlineExpiration()
        {
            LoggedInAs = accommodation1.Owner;

            var all = await accommodationService.GetAllAccommodationsAsync();

            Assert.True(all.Contains(accommodation1));

            await deletionService.DeleteAccommodationAsync(accommodation1.Id);

            all = await accommodationService.GetAllAccommodationsAsync();

            Assert.False(all.Contains(accommodation1));
        }

        [Test]
        public async Task DeleteAccommodationAsync_ShouldDeleteBookingsOfAccommodation_IfAccommodationGetsDeleted()
        {
            LoggedInAs = userAdmin;

            var allAccs = await accommodationService.GetAllAccommodationsAsync();
            var allBks = await bookingService.GetAllBookingsAsync();

            var countOfAccs = allAccs.Count;
            var countOfBks = allBks.Count;

            Assert.True(allAccs.Contains(accommodation1));
            Assert.True(allBks.Contains(booking1));
            Assert.True(allBks.Contains(booking4));
            Assert.True(allBks.Contains(booking6));
            Assert.True((booking1.Accommodation.Id == accommodation1.Id));
            Assert.True((booking4.Accommodation.Id == accommodation1.Id));
            Assert.True((booking6.Accommodation.Id == accommodation1.Id));

            await deletionService.DeleteAccommodationAsync(accommodation1.Id);

            allAccs = await accommodationService.GetAllAccommodationsAsync();
            allBks = await bookingService.GetAllBookingsAsync();

            Assert.False(allAccs.Contains(accommodation1));
            Assert.False(allBks.Contains(booking1));
            Assert.False(allBks.Contains(booking4));
            Assert.False(allBks.Contains(booking6));
            Assert.AreEqual(countOfAccs - 1, allAccs.Count);
            Assert.AreEqual(countOfBks - 3, allBks.Count);
        }

        [Test]
        public async Task DeleteAccommodationAsync_ShouldNotDeleteAnything_IfAccommodationCannotBeDeleted()
        {
            LoggedInAs = userAdmin;

            var allAccs = await accommodationService.GetAllAccommodationsAsync();
            var allBks = await bookingService.GetAllBookingsAsync();

            var countOfAccs = allAccs.Count;
            var countOfBks = allBks.Count;

            Assert.True(allAccs.Contains(accommodation1));
            Assert.True(allBks.Contains(booking1));
            Assert.True(allBks.Contains(booking4));
            Assert.True(allBks.Contains(booking6));
            Assert.True((booking1.Accommodation.Id == accommodation1.Id));
            Assert.True((booking4.Accommodation.Id == accommodation1.Id));
            Assert.True((booking6.Accommodation.Id == accommodation1.Id));

            accommodation1.CancellationDeadlineInDays = 100;

            var ex = Assert.ThrowsAsync<CancelBookingException>(async ()
                => await deletionService.DeleteAccommodationAsync(accommodation1.Id));

            allAccs = await accommodationService.GetAllAccommodationsAsync();
            allBks = await bookingService.GetAllBookingsAsync();

            Assert.True(allAccs.Contains(accommodation1));
            Assert.True(allBks.Contains(booking1));
            Assert.True(allBks.Contains(booking4));
            Assert.True(allBks.Contains(booking6));
            Assert.AreEqual(countOfAccs, allAccs.Count);
            Assert.AreEqual(countOfBks, allBks.Count);
        }

        [Test]
        public async Task DeleteAccommodationAsync_ShouldFail_IfBlockedByCancellationDeadlineExpiration()
        {
            LoggedInAs = accommodation3.Owner;

            var all = await accommodationService.GetAllAccommodationsAsync();
            var allBk = await bookingService.GetBookingsOfOwnedAccommodationAsync(accommodation3.Owner.Id);

            Assert.True(all.Contains(accommodation3));
            Assert.AreEqual(2, allBk.Count);

            var ex = Assert.ThrowsAsync<CancelBookingException>(async ()
                => await deletionService.DeleteAccommodationAsync(accommodation3.Id));

            Assert.AreEqual($"The accommodation with ID {accommodation3.Id} cannot be deleted because it has a booking with ID {booking3.Id} with a start date less than {accommodation3.CancellationDeadlineInDays} days away! Delete when no bookings are less than {accommodation3.CancellationDeadlineInDays} days away.", ex.Message);

            all = await accommodationService.GetAllAccommodationsAsync();
            allBk = await bookingService.GetBookingsOfOwnedAccommodationAsync(accommodation3.Owner.Id);

            Assert.True(all.Contains(accommodation3));
            Assert.AreEqual(2, allBk.Count);
        }

        [Test]
        public void CanAccommodationBookingsBeDeletedAsync_ShouldSucceed_IfNotBlockedByCancellationDeadlineExpiration()
        {
            LoggedInAs = accommodation2.Owner;

            var result = typeof(DeletionService)
                .GetMethod("CanAccommodationBookingsBeDeletedAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.DoesNotThrowAsync(async () => await (Task)result.Invoke(deletionService, new object[] { accommodation2.Id }));
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
        public async Task DeleteBookingAsync_ShouldSucceed_IfNotBlockedByCancellationDeadlineExpiration()
        {
            LoggedInAs = booking2.BookedBy;

            var all = await bookingService.GetAllBookingsAsync();

            Assert.True(all.Contains(booking2));

            await deletionService.DeleteBookingAsync(booking2.Id);

            all = await bookingService.GetAllBookingsAsync();

            Assert.False(all.Contains(booking2));
        }

        [Test]
        public void DeadLineExpirationAsync_ShouldSucceed_IfCancellationDeadlineHasNotExpired()
        {
            var result = typeof(DeletionService)
                .GetMethod("DeadLineExpirationAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.DoesNotThrowAsync(async () => await (Task)result.Invoke(deletionService, new object[] { booking2.Id, booking2.Accommodation.CancellationDeadlineInDays }));
        }

        [Test]
        public void DeadLineExpirationAsync_ShouldFail_IfCancellationDeadlineHasNotExpired()
        {
            var result = typeof(DeletionService)
                .GetMethod("DeadLineExpirationAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.ThrowsAsync<CancelBookingException>(async ()
                => await (Task)result.Invoke(deletionService, new object[] { booking3.Id, booking3.Accommodation.CancellationDeadlineInDays }));

            Assert.AreEqual($"Cannot change the booking with ID {booking3.Id} because the start date of the booking is less than {booking3.Accommodation.CancellationDeadlineInDays} days away!", ex.Message);
        }

        [Test]
        public async Task ResetAvailableStatusAfterDeletingBookingAsync_ShouldSucceed_IfBookingDatesCancelled()
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
            await (Task)result.Invoke(deletionService, new object[] { booking2.Id });

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
