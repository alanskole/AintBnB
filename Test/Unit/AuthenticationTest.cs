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
        public void AdminChecker_ShouldReturn_TrueIfAdminIsLoggedIn()
        {
            LoggedInAs = admin;

            Assert.IsTrue(AdminChecker());
        }

        [TestMethod]
        public void AdminChecker_ShouldReturn_FalseIfLoggedInUserIsNotAdmin()
        {
            LoggedInAs = customer1;

            Assert.IsFalse(AdminChecker());
        }

        [TestMethod]
        public void AdminChecker_ShouldReturnFalse_IfNoOneIsLoggedIn()
        {
            LoggedInAs = null;

            Assert.IsFalse(AdminChecker());
        }

        [TestMethod]
        public void CorrectUserOrAdmin_ShouldReturn_TrueIfLoggedInUserIsAdminOrOrTheUserBeingChecked()
        {
            LoggedInAs = customer1;

            Assert.IsTrue(CorrectUserOrAdmin(customer1.Id));

            LoggedInAs = admin;

            Assert.IsTrue(CorrectUserOrAdmin(customer1.Id));
        }

        [TestMethod]
        public void CorrectUserOrAdmin_ShouldReturn_FalseIfLoggedInUserIsCustomerButUserInMethodParameterIsNotTheLoggedInCustomer()
        {
            LoggedInAs = customer1;

            Assert.IsFalse(CorrectUserOrAdmin(customer2.Id));
        }

        [TestMethod]
        public void CorrectUserOrAdmin_ShouldFail_NoOneLoggedIn()
        {
            LoggedInAs = null;

            Assert.IsFalse(CorrectUserOrAdmin(customer1.Id));
        }

        [TestMethod]
        public void AdminChecker_ShouldReturn_TrueIfLoggedInUserIsAdminOr()
        {
            LoggedInAs = admin;

            Assert.IsTrue(AdminChecker());
        }

        [TestMethod]
        public void AdminChecker_ShouldReturn_FalseIfLoggedInUserIsNotAdminOr()
        {
            LoggedInAs = customer1;

            Assert.IsFalse(AdminChecker());
        }

        [TestMethod]
        public void CorrectUserOrOwnerOrAdmin_ShouldReturn_TrueIfLoggedInUserIsTheSameAsAnyOfTheMethodParameters()
        {
            LoggedInAs = customer1;

            Assert.IsTrue(CorrectUserOrOwnerOrAdmin(customer1.Id, customer2));

            LoggedInAs = customer2;

            Assert.IsTrue(CorrectUserOrOwnerOrAdmin(customer1.Id, customer2));
        }

        [TestMethod]
        public void CorrectUserOrOwnerOrAdmin_ShouldReturn_TrueIfLoggedInUserIsAdmin()
        {
            LoggedInAs = admin;

            Assert.IsTrue(CorrectUserOrOwnerOrAdmin(customer1.Id, customer2));
        }

        [TestMethod]
        public void CorrectUserOrOwnerOrAdmin_ShouldReturn_FalseIfLoggedInUserIsNotTheSameAsAnyOfTheMethodParameters()
        {
            LoggedInAs = customer2;

            Assert.IsFalse(CorrectUserOrOwnerOrAdmin(customer1.Id, customer3));
        }

        [TestMethod]
        public void CorrectUserOrOwnerOrAdmin_ShouldFail_NoOneLoggedIn()
        {
            LoggedInAs = null;

            var ex = Assert.ThrowsException<LoginException>(()
                => CorrectUserOrOwnerOrAdmin(customer2.Id, customer1));

            Assert.AreEqual("Not logged in!", ex.Message);
        }


        [TestMethod]
        public void CheckIfUserIsAllowedToPerformAction_ShouldReturn_TrueIfLoggedInUserIsAdminAndUserInMethodParameterIsCustomer()
        {
            LoggedInAs = admin;

            Assert.IsTrue(CheckIfUserIsAllowedToPerformAction(customer2));
        }

        [TestMethod]
        public void CheckIfUserIsAllowedToPerformAction_ShouldReturn_TrueIfLoggedInUserIsCustomerAndIsTheSameAsTheUserInTheMethodParameter()
        {
            LoggedInAs = customer2;

            Assert.IsTrue(CheckIfUserIsAllowedToPerformAction(customer2));
        }

        [TestMethod]
        public void CheckIfUserIsAllowedToPerformAction_ShouldReturn_FalseIfTheUserInTheMethodParameterIsNotCustomer()
        {
            LoggedInAs = admin;

            Assert.IsFalse(CheckIfUserIsAllowedToPerformAction(admin));
        }

        [TestMethod]
        public void CheckIfUserIsAllowedToPerformAction_ShouldFail_NoOneLoggedIn()
        {
            LoggedInAs = null;

            var ex = Assert.ThrowsException<LoginException>(()
                => CheckIfUserIsAllowedToPerformAction(customer1));

            Assert.AreEqual("Not logged in!", ex.Message);
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
            LoggedInAs = null;
            await SetupDatabaseForTestingAsync();
            SetupTestClasses();
            await CreateDummyUsersAsync();

            var result = typeof(Authentication)
                .GetMethod("LoginUser", BindingFlags.NonPublic | BindingFlags.Static);

            var all = await userService.GetAllUsersForLoginAsync();

            result.Invoke(null, new object[] { userCustomer1.UserName, "aaaaaa", all });
        }

        [TestMethod]
        public async Task LoginUser_ShouldFail_IfTheUsernameIsWrong()
        {
            LoggedInAs = null;
            await SetupDatabaseForTestingAsync();
            SetupTestClasses();
            await CreateDummyUsersAsync();

            var result = typeof(Authentication)
                .GetMethod("LoginUser", BindingFlags.NonPublic | BindingFlags.Static);

            var ex = await Assert.ThrowsExceptionAsync<TargetInvocationException>(async ()
                => await (Task)result.Invoke(null, new object[] { "ssssssssssssssss", "aaaaaa", await userService.GetAllUsersForLoginAsync() }));


            Assert.AreEqual(ex.InnerException.GetType(), typeof(LoginException));

            Assert.AreEqual("Username and/or password not correct!", ex.InnerException.Message);
        }

        [TestMethod]
        public async Task LoginUser_ShouldFail_IfThePasswordIsWrong()
        {
            LoggedInAs = null;
            await SetupDatabaseForTestingAsync();
            SetupTestClasses();
            await CreateDummyUsersAsync();

            var result = typeof(Authentication)
                .GetMethod("LoginUser", BindingFlags.NonPublic | BindingFlags.Static);

            var ex = await Assert.ThrowsExceptionAsync<TargetInvocationException>(async ()
                => await (Task)result.Invoke(null, new object[] { userCustomer1.UserName, "blablablabla", await userService.GetAllUsersForLoginAsync() }));


            Assert.AreEqual(ex.InnerException.GetType(), typeof(LoginException));

            Assert.AreEqual("Username and/or password not correct!", ex.InnerException.Message);
        }

        [TestMethod]
        public async Task TryToLogin_ShouldFail_IfAlreadyLoggedIn()
        {
            await SetupDatabaseForTestingAsync();
            SetupTestClasses();
            await CreateDummyUsersAsync();

            LoggedInAs = userCustomer1;

            var ex = await Assert.ThrowsExceptionAsync<AlreadyLoggedInException>(async ()
                => TryToLogin(userCustomer1.UserName, "blablablabla", await userService.GetAllUsersForLoginAsync()));

            Assert.AreEqual("Already logged in!", ex.Message);
        }
    }
}
