using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Imp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Threading.Tasks;

namespace Test.Unit
{
    [TestClass]
    public class DeletionServiceTest : TestBase
    {
        [TestInitialize]
        public async Task SetUp()
        {
            await SetupDatabaseForTestingAsync();
            SetupTestClasses();
            await CreateDummyUsersAsync();
            await CreateDummyAccommodationAsync();
            await CreateDummyBookingAsync();
        }

        [TestCleanup]
        public async Task TearDown()
        {
            await DisposeAsync();
        }

        [TestMethod]
        public async Task DeleteUserAsync_ShouldFail_IfTheUserHasABookingOfTheirAccommodationThatCannotBeDeletedBecauseOfCancellationDeadlineExpired()
        {
            var all = await bookingService.GetBookingsOfOwnedAccommodationAsync(booking1.BookedBy.Id);

            Assert.AreEqual(2, all.Count);

            var ex = await Assert.ThrowsExceptionAsync<CancelBookingException>(async ()
                => await deletionService.DeleteUserAsync(booking1.BookedBy.Id));

            Assert.AreEqual($"The user with ID {booking1.BookedBy.Id} cannot be deleted because it has an accommodation with ID {booking3.Accommodation.Id} that has bookings that can't be deleted because of the cancellation deadline of the accommodation. Delete when no bookings of the accommodation have surpassed the cancellation deadline of {booking3.Accommodation.CancellationDeadlineInDays} days.", ex.Message);

            all = await bookingService.GetBookingsOfOwnedAccommodationAsync(booking1.BookedBy.Id);

            Assert.AreEqual(2, all.Count);
        }

        [TestMethod]
        public async Task DeleteUserAsync_ShouldFail_IfTheUserHasABookingThatCannotBeDeletedBecauseOfCancellationDeadlineExpiredAsync()
        {
            var ex = await Assert.ThrowsExceptionAsync<CancelBookingException>(async ()
                => await deletionService.DeleteUserAsync(booking3.BookedBy.Id));

            Assert.AreEqual($"The user cannot be deleted because it has a booking with ID {booking3.Id} with a start date less than {booking3.Accommodation.CancellationDeadlineInDays} days away! Delete when no bookings are less than {booking3.Accommodation.CancellationDeadlineInDays} days away.", ex.Message);
        }

        [TestMethod]
        public async Task DeleteUserAsync_ShouldDeleteNothing_IfUserCannotBeDeleted()
        {
            var allUsr = await userService.GetAllUsersAsync();
            var allAcc = await accommodationService.GetAllAccommodationsAsync();
            var allBk = await bookingService.GetAllInSystemAsync();

            Assert.AreEqual(4, allUsr.Count);
            Assert.AreEqual(4, allAcc.Count);
            Assert.AreEqual(6, allBk.Count);

            var ex = await Assert.ThrowsExceptionAsync<CancelBookingException>(async ()
                => await deletionService.DeleteUserAsync(userCustomer1.Id));

            Assert.AreEqual($"The user cannot be deleted because it has a booking with ID {booking3.Id} with a start date less than {booking3.Accommodation.CancellationDeadlineInDays} days away! Delete when no bookings are less than {booking3.Accommodation.CancellationDeadlineInDays} days away.", ex.Message);

            allAcc = await accommodationService.GetAllAccommodationsAsync();
            allBk = await bookingService.GetAllInSystemAsync();

            Assert.AreEqual(4, allUsr.Count);
            Assert.AreEqual(4, allAcc.Count);
            Assert.AreEqual(6, allBk.Count);
        }

        [TestMethod]
        public async Task DeleteUserAsync_ShouldSucceed_IfNoCancellationDeadlineExpiredOnBookingsOrAccommodations()
        {
            var allUsr = await userService.GetAllUsersAsync();

            Assert.IsTrue(allUsr.Contains(userCustomer2));

            booking3.Accommodation.CancellationDeadlineInDays = 1;

            await deletionService.DeleteUserAsync(userCustomer2.Id);

            allUsr = await userService.GetAllUsersAsync();

            Assert.IsFalse(allUsr.Contains(userCustomer2));
        }

        [TestMethod]
        public async Task DeleteUserAsync_ShouldSucceed_IfTheCheckoutDateOfTheBookingOfTheUserToBeDeletedIsInThePast()
        {
            var all = await userService.GetAllUsersAsync();

            Assert.IsTrue(all.Contains(userCustomer3));

            await deletionService.DeleteUserAsync(booking4.BookedBy.Id);

            all = await userService.GetAllUsersAsync();

            Assert.IsFalse(all.Contains(userCustomer3));
        }

        [TestMethod]
        public async Task DeleteUserAsync_ShouldDeleteTheAccommodationsBookingsAndBookingsOfAccommodations_OfTheDeletedUser()
        {
            var allUsr = await userService.GetAllUsersAsync();
            var allAcc = await accommodationService.GetAllAccommodationsAsync();
            var allBk = await bookingService.GetAllInSystemAsync();

            var countOfUsr = allUsr.Count;
            var countOfAcc = allAcc.Count;
            var countOfBk = allBk.Count;

            Assert.IsTrue(allUsr.Contains(userCustomer3));
            Assert.IsTrue(allAcc.Contains(accommodation4));
            Assert.IsTrue((accommodation4.Owner.Id == userCustomer3.Id));
            Assert.IsTrue(allBk.Contains(booking4));
            Assert.IsTrue(allBk.Contains(booking5));
            Assert.IsTrue(allBk.Contains(booking6));
            Assert.IsTrue((booking4.BookedBy.Id == userCustomer3.Id));
            Assert.IsTrue((booking5.Accommodation.Owner.Id == userCustomer3.Id));
            Assert.IsTrue((booking6.BookedBy.Id == userCustomer3.Id));

            await deletionService.DeleteUserAsync(userCustomer3.Id);

            allUsr = await userService.GetAllUsersAsync();
            allAcc = await accommodationService.GetAllAccommodationsAsync();
            allBk = await bookingService.GetAllInSystemAsync();

            Assert.IsFalse(allUsr.Contains(userCustomer3));
            Assert.IsFalse(allAcc.Contains(accommodation4));
            Assert.IsFalse(allBk.Contains(booking4));
            Assert.IsFalse(allBk.Contains(booking5));
            Assert.IsFalse(allBk.Contains(booking6));
            Assert.AreEqual(countOfUsr - 1, allUsr.Count);
            Assert.AreEqual(countOfAcc - 1, allAcc.Count);
            Assert.AreEqual(countOfBk - 3, allBk.Count);
        }

        [TestMethod]
        public void CheckIfUserCanBeDeleted_ShouldFail_IfAdminAccountTriesToGetDeleted()
        {
            var result = typeof(DeletionService)
                .GetMethod("CheckIfUserCanBeDeleted", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.ThrowsException<TargetInvocationException>(()
                => result.Invoke(deletionService, new object[] { userAdmin }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(AccessException));

            Assert.AreEqual("Admin cannot be deleted!", ex.InnerException.Message);
        }

        [TestMethod]
        public async Task DeleteUsersAccommodationsAsync_ShouldSucceed_IfNotBlockedByCancellationDeadline()
        {
            booking3.Accommodation.CancellationDeadlineInDays = 1;

            var result = typeof(DeletionService)
                .GetMethod("DeleteUsersAccommodationsAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            await (Task)result.Invoke(deletionService, new object[] { booking3.Accommodation.Owner.Id });
        }

        [TestMethod]
        public async Task DeleteUsersAccommodationsAsync_ShouldFail_IfBlockedByCancellationDeadlineAsync()
        {
            var result = typeof(DeletionService)
                .GetMethod("DeleteUsersAccommodationsAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = await Assert.ThrowsExceptionAsync<CancelBookingException>(async ()
                => await (Task)result.Invoke(deletionService, new object[] { booking3.Accommodation.Owner.Id }));

            Assert.AreEqual($"The user with ID {booking1.BookedBy.Id} cannot be deleted because it has an accommodation with ID {booking3.Accommodation.Id} that has bookings that can't be deleted because of the cancellation deadline of the accommodation. Delete when no bookings of the accommodation have surpassed the cancellation deadline of {booking3.Accommodation.CancellationDeadlineInDays} days.", ex.Message);
        }

        [TestMethod]
        public async Task DeleteUsersBookingsAsync_ShouldSucceed_IfNotBlockedByCancellationDeadline()
        {
            booking3.Accommodation.CancellationDeadlineInDays = 1;

            var result = typeof(DeletionService)
                .GetMethod("DeleteUsersBookingsAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            await (Task)result.Invoke(deletionService, new object[] { booking3.BookedBy.Id });
        }

        [TestMethod]
        public async Task DeleteUsersBookingsAsync_ShouldFail_IfBlockedByCancellationDeadlineAsync()
        {
            var result = typeof(DeletionService)
                .GetMethod("DeleteUsersBookingsAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = await Assert.ThrowsExceptionAsync<CancelBookingException>(async ()
                => await (Task)result.Invoke(deletionService, new object[] { booking3.BookedBy.Id }));

            Assert.AreEqual("The user cannot be deleted because it has a booking with ID 3 with a start date less than 3 days away! Delete when no bookings are less than 3 days away.", ex.Message);
        }

        [TestMethod]
        public async Task DeleteAccommodationAsync_ShouldSucceed_IfNotBlockedByCancellationDeadlineExpiration()
        {
            var all = await accommodationService.GetAllAccommodationsAsync();

            Assert.IsTrue(all.Contains(accommodation1));

            await deletionService.DeleteAccommodationAsync(accommodation1.Id);

            all = await accommodationService.GetAllAccommodationsAsync();

            Assert.IsFalse(all.Contains(accommodation1));
        }

        [TestMethod]
        public async Task DeleteAccommodationAsync_ShouldDeleteBookingsOfAccommodation_IfAccommodationGetsDeleted()
        {
            var allAccs = await accommodationService.GetAllAccommodationsAsync();
            var allBks = await bookingService.GetAllInSystemAsync();

            var countOfAccs = allAccs.Count;
            var countOfBks = allBks.Count;

            Assert.IsTrue(allAccs.Contains(accommodation1));
            Assert.IsTrue(allBks.Contains(booking1));
            Assert.IsTrue(allBks.Contains(booking4));
            Assert.IsTrue(allBks.Contains(booking6));
            Assert.IsTrue((booking1.Accommodation.Id == accommodation1.Id));
            Assert.IsTrue((booking4.Accommodation.Id == accommodation1.Id));
            Assert.IsTrue((booking6.Accommodation.Id == accommodation1.Id));

            await deletionService.DeleteAccommodationAsync(accommodation1.Id);

            allAccs = await accommodationService.GetAllAccommodationsAsync();
            allBks = await bookingService.GetAllInSystemAsync();

            Assert.IsFalse(allAccs.Contains(accommodation1));
            Assert.IsFalse(allBks.Contains(booking1));
            Assert.IsFalse(allBks.Contains(booking4));
            Assert.IsFalse(allBks.Contains(booking6));
            Assert.AreEqual(countOfAccs - 1, allAccs.Count);
            Assert.AreEqual(countOfBks - 3, allBks.Count);
        }

        [TestMethod]
        public async Task DeleteAccommodationAsync_ShouldNotDeleteAnything_IfAccommodationCannotBeDeleted()
        {
            var allAccs = await accommodationService.GetAllAccommodationsAsync();
            var allBks = await bookingService.GetAllInSystemAsync();

            var countOfAccs = allAccs.Count;
            var countOfBks = allBks.Count;

            Assert.IsTrue(allAccs.Contains(accommodation1));
            Assert.IsTrue(allBks.Contains(booking1));
            Assert.IsTrue(allBks.Contains(booking4));
            Assert.IsTrue(allBks.Contains(booking6));
            Assert.IsTrue((booking1.Accommodation.Id == accommodation1.Id));
            Assert.IsTrue((booking4.Accommodation.Id == accommodation1.Id));
            Assert.IsTrue((booking6.Accommodation.Id == accommodation1.Id));

            accommodation1.CancellationDeadlineInDays = 100;

            var ex = await Assert.ThrowsExceptionAsync<CancelBookingException>(async ()
                => await deletionService.DeleteAccommodationAsync(accommodation1.Id));

            allAccs = await accommodationService.GetAllAccommodationsAsync();
            allBks = await bookingService.GetAllInSystemAsync();

            Assert.IsTrue(allAccs.Contains(accommodation1));
            Assert.IsTrue(allBks.Contains(booking1));
            Assert.IsTrue(allBks.Contains(booking4));
            Assert.IsTrue(allBks.Contains(booking6));
            Assert.AreEqual(countOfAccs, allAccs.Count);
            Assert.AreEqual(countOfBks, allBks.Count);
        }

        [TestMethod]
        public async Task DeleteAccommodationAsync_ShouldFail_IfBlockedByCancellationDeadlineExpiration()
        {
            var all = await accommodationService.GetAllAccommodationsAsync();
            var allBk = await bookingService.GetBookingsOfOwnedAccommodationAsync(accommodation3.Owner.Id);

            Assert.IsTrue(all.Contains(accommodation3));
            Assert.AreEqual(2, allBk.Count);

            var ex = await Assert.ThrowsExceptionAsync<CancelBookingException>(async ()
                => await deletionService.DeleteAccommodationAsync(accommodation3.Id));

            Assert.AreEqual($"The accommodation with ID {accommodation3.Id} cannot be deleted because it has a booking with ID {booking3.Id} with a start date less than {accommodation3.CancellationDeadlineInDays} days away! Delete when no bookings are less than {accommodation3.CancellationDeadlineInDays} days away.", ex.Message);

            all = await accommodationService.GetAllAccommodationsAsync();
            allBk = await bookingService.GetBookingsOfOwnedAccommodationAsync(accommodation3.Owner.Id);

            Assert.IsTrue(all.Contains(accommodation3));
            Assert.AreEqual(2, allBk.Count);
        }

        [TestMethod]
        public async Task CanAccommodationBookingsBeDeletedAsync_ShouldSucceed_IfNotBlockedByCancellationDeadlineExpiration()
        {
            var result = typeof(DeletionService)
                .GetMethod("CanAccommodationBookingsBeDeletedAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            await (Task)result.Invoke(deletionService, new object[] { accommodation2.Id });
        }

        [TestMethod]
        public async Task DeleteBookingAsync_ShouldSucceed_IfNotBlockedByCancellationDeadlineExpiration()
        {
            var all = await bookingService.GetAllInSystemAsync();

            Assert.IsTrue(all.Contains(booking2));

            await deletionService.DeleteBookingAsync(booking2.Id);

            all = await bookingService.GetAllInSystemAsync();

            Assert.IsFalse(all.Contains(booking2));
        }

        [TestMethod]
        public async Task DeadLineExpirationAsync_ShouldSucceed_IfCancellationDeadlineHasNotExpired()
        {
            var result = typeof(DeletionService)
                .GetMethod("DeadLineExpirationAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            await (Task)result.Invoke(deletionService, new object[] { booking2.Id, booking2.Accommodation.CancellationDeadlineInDays });
        }

        [TestMethod]
        public async Task DeadLineExpirationAsync_ShouldFail_IfCancellationDeadlineHasNotExpiredAsync()
        {
            var result = typeof(DeletionService)
                .GetMethod("DeadLineExpirationAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = await Assert.ThrowsExceptionAsync<CancelBookingException>(async ()
                => await (Task)result.Invoke(deletionService, new object[] { booking3.Id, booking3.Accommodation.CancellationDeadlineInDays }));

            Assert.AreEqual($"Cannot change the booking with ID {booking3.Id} because the start date of the booking is less than {booking3.Accommodation.CancellationDeadlineInDays} days away!", ex.Message);
        }

        [TestMethod]
        public async Task ResetAvailableStatusAfterDeletingBookingAsync_ShouldSucceed_IfBookingDatesCancelled()
        {
            Assert.IsFalse(booking2.Accommodation.Schedule[booking2.Dates[0]]);
            Assert.IsFalse(booking2.Accommodation.Schedule[booking2.Dates[1]]);
            Assert.IsFalse(booking2.Accommodation.Schedule[booking2.Dates[2]]);
            Assert.IsFalse(booking2.Accommodation.Schedule[booking2.Dates[3]]);
            Assert.IsFalse(booking2.Accommodation.Schedule[booking2.Dates[4]]);
            Assert.IsFalse(booking2.Accommodation.Schedule[booking2.Dates[5]]);
            Assert.IsFalse(booking2.Accommodation.Schedule[booking2.Dates[6]]);
            Assert.IsFalse(booking2.Accommodation.Schedule[booking2.Dates[7]]);
            Assert.IsFalse(booking2.Accommodation.Schedule[booking2.Dates[8]]);
            Assert.IsFalse(booking2.Accommodation.Schedule[booking2.Dates[9]]);
            Assert.IsFalse(booking2.Accommodation.Schedule[booking2.Dates[10]]);
            Assert.IsFalse(booking2.Accommodation.Schedule[booking2.Dates[11]]);

            var result = typeof(DeletionService)
                .GetMethod("ResetAvailableStatusAfterDeletingBookingAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)result.Invoke(deletionService, new object[] { booking2.Id });

            Assert.IsTrue(booking2.Accommodation.Schedule[booking2.Dates[0]]);
            Assert.IsTrue(booking2.Accommodation.Schedule[booking2.Dates[1]]);
            Assert.IsTrue(booking2.Accommodation.Schedule[booking2.Dates[2]]);
            Assert.IsTrue(booking2.Accommodation.Schedule[booking2.Dates[3]]);
            Assert.IsTrue(booking2.Accommodation.Schedule[booking2.Dates[4]]);
            Assert.IsTrue(booking2.Accommodation.Schedule[booking2.Dates[5]]);
            Assert.IsTrue(booking2.Accommodation.Schedule[booking2.Dates[6]]);
            Assert.IsTrue(booking2.Accommodation.Schedule[booking2.Dates[7]]);
            Assert.IsTrue(booking2.Accommodation.Schedule[booking2.Dates[8]]);
            Assert.IsTrue(booking2.Accommodation.Schedule[booking2.Dates[9]]);
            Assert.IsTrue(booking2.Accommodation.Schedule[booking2.Dates[10]]);
            Assert.IsTrue(booking2.Accommodation.Schedule[booking2.Dates[11]]);
        }
    }
}
