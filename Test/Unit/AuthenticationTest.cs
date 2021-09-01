using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Helpers;
using AintBnB.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Threading.Tasks;
using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace Test.Unit
{
    [TestClass]
    public class AuthenticationTest : TestBase
    {
        private User admin = new User
        {
            Id = 1,
            UserName = "admin",
            Password = HashPassword("aaaaaa"),
            FirstName = "Ad",
            LastName = "Min",
            UserType = UserTypes.Admin
        };

        private User customer1 = new User
        {
            Id = 4,
            UserName = "cust1",
            Password = HashPassword("aaaaaa"),
            FirstName = "First",
            LastName = "Cust",
            UserType = UserTypes.Customer
        };

        private User customer2 = new User
        {
            Id = 5,
            UserName = "cust2",
            Password = HashPassword("aaaaaa"),
            FirstName = "Second",
            LastName = "Cust",
            UserType = UserTypes.Customer
        };

        private User customer3 = new User
        {
            Id = 7,
            UserName = "last",
            Password = HashPassword("aaaaaa"),
            FirstName = "last",
            LastName = "Cust",
            UserType = UserTypes.Customer
        };

        [TestMethod]
        public void AdminChecker_ShouldReturn_TrueIfAdmin()
        {
            Assert.IsTrue(AdminChecker(admin.UserType));
        }

        [TestMethod]
        public void AdminChecker_ShouldReturn_FalseIfNotAdmin()
        {
            Assert.IsFalse(AdminChecker(customer1.UserType));
        }

        [TestMethod]
        public void CorrectUserOrAdmin_ShouldReturn_TrueIfUserIsAdminOrOrTheUserBeingChecked()
        {
            Assert.IsTrue(CorrectUserOrAdmin(customer1.Id, customer1.Id, customer1.UserType));

            Assert.IsTrue(CorrectUserOrAdmin(customer1.Id, admin.Id, admin.UserType));
        }

        [TestMethod]
        public void CorrectUserOrAdmin_ShouldReturn_FalseIfTheUserIsNotAdminTryingToAccessDataOwnedByAnotherUser()
        {
            Assert.IsFalse(CorrectUserOrAdmin(customer2.Id, customer1.Id, customer1.UserType));
        }

        [TestMethod]
        public void CorrectUserOrOwnerOrAdmin_ShouldReturn_TrueIfTheUserIsTheSameAsAnyOfTheMethodParameters()
        {
            Assert.IsTrue(CorrectUserOrOwnerOrAdmin(customer1.Id, customer3.Id, customer1.Id, customer1.UserType));

            Assert.IsTrue(CorrectUserOrOwnerOrAdmin(customer1.Id, customer3.Id, customer3.Id, customer3.UserType));
        }

        [TestMethod]
        public void CorrectUserOrOwnerOrAdmin_ShouldReturn_TrueIfTheUserIsAdmin()
        {
            Assert.IsTrue(CorrectUserOrOwnerOrAdmin(customer1.Id, customer3.Id, admin.Id, admin.UserType));
        }

        [TestMethod]
        public void CorrectUserOrOwnerOrAdmin_ShouldReturn_FalseIfTheUserIsNotTheSameAsAnyOfTheMethodParameters()
        {
            Assert.IsFalse(CorrectUserOrOwnerOrAdmin(customer1.Id, customer3.Id, customer2.Id, customer2.UserType));
        }

        [TestMethod]
        public void CheckIfUserIsAllowedToPerformAction_ShouldReturn_TrueIfTheUserIsAdminAndUserInMethodParameterIsCustomer()
        {
            Assert.IsTrue(CheckIfUserIsAllowedToPerformAction(customer1, admin.Id, admin.UserType));
        }

        [TestMethod]
        public void CheckIfUserIsAllowedToPerformAction_ShouldReturn_TrueIfTheUserIsCustomerAndIsTheSameAsTheUserInTheMethodParameter()
        {
            Assert.IsTrue(CheckIfUserIsAllowedToPerformAction(customer1, customer1.Id, customer1.UserType));
        }

        [TestMethod]
        public void CheckIfUserIsAllowedToPerformAction_ShouldReturn_FalseIfTheUserInTheMethodParameterIsNotCustomer()
        {
            Assert.IsFalse(CheckIfUserIsAllowedToPerformAction(admin, admin.Id, admin.UserType));
        }

        [TestMethod]
        public void HashPassword_ShouldReturn_ADifferentStringThanTheOneInTheMethodParameter()
        {
            string pass = "a";

            Assert.AreNotEqual(pass, HashPassword(pass));
        }

        [TestMethod]
        public void HashPassword_ShouldReturn_TrueIfUnhashedPasswordMatchesTheHashedPassword()
        {
            string pass = "a";
            string hashed = HashPassword(pass);

            Assert.IsTrue(VerifyPasswordHash(pass, hashed));
        }

        [TestMethod]
        public void HashPassword_ShouldReturn_FalseIfUnhashedPasswordDoesNotMatchesTheHashedPassword()
        {
            string pass = "a";
            string hashed = HashPassword(pass);
            string wrong = pass + "a";

            Assert.IsFalse(VerifyPasswordHash(wrong, hashed));
        }


        [TestMethod]
        public async Task LoginUser_ShouldSucceed_IfTheUserLoggingInHasEnteredCorrectUsernameAndPassword()
        {
            await SetupDatabaseForTestingAsync();
            SetupTestClasses();
            await CreateDummyUsersAsync();

            var all = await userService.GetAllUsersAsync();

            TryToLogin(userCustomer1.UserName, "aaaaaa", all);
        }

        [TestMethod]
        public async Task LoginUser_ShouldFail_IfTheUsernameIsWrong()
        {
            await SetupDatabaseForTestingAsync();
            SetupTestClasses();
            await CreateDummyUsersAsync();

            var all = await userService.GetAllUsersAsync();

            var ex = Assert.ThrowsException<LoginException>( ()
                => TryToLogin("ssssssssssssssss", "aaaaaa", all));

            Assert.AreEqual("Username and/or password not correct!", ex.Message);
        }

        [TestMethod]
        public async Task LoginUser_ShouldFail_IfThePasswordIsWrong()
        {
            await SetupDatabaseForTestingAsync();
            SetupTestClasses();
            await CreateDummyUsersAsync();

            var all = await userService.GetAllUsersAsync();

            
            var ex = Assert.ThrowsException<LoginException>( ()
                => TryToLogin(userCustomer1.UserName, "blblblblbla", all));

            Assert.AreEqual("Username and/or password not correct!", ex.Message);
        }
    }
}
