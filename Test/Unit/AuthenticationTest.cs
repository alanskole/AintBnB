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

        private User employee1 = new User
        {
            Id = 2,
            UserName = "emp",
            Password = HashPassword("aaaaaa"),
            FirstName = "Em",
            LastName = "Pl",
            UserType = UserTypes.Employee
        };

        private User employeeRequester = new User
        {
            Id = 3,
            UserName = "empreq",
            Password = HashPassword("aaaaaa"),
            FirstName = "Req",
            LastName = "Emp",
            UserType = UserTypes.RequestToBeEmployee
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

        private User employee2 = new User
        {
            Id = 6,
            UserName = "emp2",
            Password = HashPassword("aaaaaa"),
            FirstName = "Second",
            LastName = "Emp",
            UserType = UserTypes.Employee
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

            LoggedInAs = employee1;

            Assert.IsFalse(AdminChecker());

            LoggedInAs = employeeRequester;

            Assert.IsFalse(AdminChecker());
        }

        [TestMethod]
        public void AdminChecker_ShouldFail_IfNoOneIsLoggedIn()
        {
            LoggedInAs = null;

            var ex = Assert.ThrowsException<LoginException>(()
                => AdminChecker());

            Assert.AreEqual("Not logged in!", ex.Message);
        }

        [TestMethod]
        public void EmployeeChecker_ShouldReturn_TrueIfEmployeeIsLoggedIn()
        {
            LoggedInAs = employee1;

            Assert.IsTrue(EmployeeChecker());
        }

        [TestMethod]
        public void EmployeeChecker_ShouldReturn_FalseIfLoggedInUserIsNotEmployee()
        {
            LoggedInAs = customer1;

            Assert.IsFalse(EmployeeChecker());

            LoggedInAs = admin;

            Assert.IsFalse(EmployeeChecker());

            LoggedInAs = employeeRequester;

            Assert.IsFalse(EmployeeChecker());
        }

        [TestMethod]
        public void EmployeeChecker_ShouldFail_IfNoOneIsLoggedIn()
        {
            LoggedInAs = null;

            var ex = Assert.ThrowsException<LoginException>(()
                => EmployeeChecker());

            Assert.AreEqual("Not logged in!", ex.Message);
        }

        [TestMethod]
        public void CorrectUserOrAdminOrEmployee_ShouldReturn_TrueIfLoggedInUserIsAdminOrEmployeeOrTheUserBeingChecked()
        {
            LoggedInAs = customer1;

            Assert.IsTrue(CorrectUserOrAdminOrEmployee(customer1));

            LoggedInAs = admin;

            Assert.IsTrue(CorrectUserOrAdminOrEmployee(customer1));

            LoggedInAs = employee1;

            Assert.IsTrue(CorrectUserOrAdminOrEmployee(customer1));
        }

        [TestMethod]
        public void CorrectUserOrAdminOrEmployee_ShouldReturn_FalseIfLoggedInUserIsCustomerButUserInMethodParameterIsNotTheLoggedInCustomer()
        {
            LoggedInAs = customer1;

            Assert.IsFalse(CorrectUserOrAdminOrEmployee(customer2));
        }

        [TestMethod]
        public void CorrectUserOrAdminOrEmployee_ShouldReturn_FalseIfLoggedInUserIsEmployeeButUserInMethodParameterIsNotTheLoggedInEmployeeOrACustomer()
        {
            LoggedInAs = employee1;

            Assert.IsFalse(CorrectUserOrAdminOrEmployee(admin));
            Assert.IsFalse(CorrectUserOrAdminOrEmployee(employee2));
            Assert.IsFalse(CorrectUserOrAdminOrEmployee(employeeRequester));
        }

        [TestMethod]
        public void CorrectUserOrAdminOrEmployee_ShouldFail_NoOneLoggedIn()
        {
            LoggedInAs = null;

            var ex = Assert.ThrowsException<LoginException>(()
                => CorrectUserOrAdminOrEmployee(customer1));

            Assert.AreEqual("Not logged in!", ex.Message);
        }

        [TestMethod]
        public void HasElevatedRights_ShouldReturn_TrueIfLoggedInUserIsAdminOrEmployee()
        {
            LoggedInAs = employee1;

            Assert.IsTrue(HasElevatedRights());

            LoggedInAs = admin;

            Assert.IsTrue(HasElevatedRights());
        }

        [TestMethod]
        public void HasElevatedRights_ShouldReturn_FalseIfLoggedInUserIsNotAdminOrEmployee()
        {
            LoggedInAs = customer1;

            Assert.IsFalse(HasElevatedRights());

            LoggedInAs = employeeRequester;

            Assert.IsFalse(HasElevatedRights());
        }

        [TestMethod]
        public void HasElevatedRights_ShouldFail_NoOneLoggedIn()
        {
            LoggedInAs = null;

            var ex = Assert.ThrowsException<LoginException>(()
                => HasElevatedRights());

            Assert.AreEqual("Not logged in!", ex.Message);
        }

        [TestMethod]
        public void CorrectUserOrAdmin_ShouldReturn_TrueIfLoggedInUserIsAdminOrTheUserBeingChecked()
        {
            LoggedInAs = customer1;

            Assert.IsTrue(CorrectUserOrAdmin(customer1.Id));

            LoggedInAs = admin;

            Assert.IsTrue(CorrectUserOrAdmin(customer1.Id));
        }

        [TestMethod]
        public void CorrectUserOrAdmin_ShouldReturn_FalseIfLoggedInUserIsEmployee()
        {
            LoggedInAs = employee1;

            Assert.IsFalse(CorrectUserOrAdmin(customer2.Id));
        }

        [TestMethod]
        public void CorrectUserOrAdmin_ShouldReturn_FalseIfLoggedInUserIsCustomerButUserInMethodParameterIsNotTheLoggedInCustomer()
        {
            LoggedInAs = customer2;

            Assert.IsFalse(CorrectUserOrAdmin(customer1.Id));
        }

        [TestMethod]
        public void CorrectUserOrAdmin_ShouldFail_NoOneLoggedIn()
        {
            LoggedInAs = null;

            var ex = Assert.ThrowsException<LoginException>(()
                => CorrectUserOrAdmin(customer1.Id));

            Assert.AreEqual("Not logged in!", ex.Message);
        }

        [TestMethod]
        public void CorrectUserOrOwnerOrAdminOrEmployee_ShouldReturn_TrueIfLoggedInUserIsTheSameAsAnyOfTheMethodParameters()
        {
            LoggedInAs = customer1;

            Assert.IsTrue(CorrectUserOrOwnerOrAdminOrEmployee(customer1.Id, customer2));

            LoggedInAs = customer2;

            Assert.IsTrue(CorrectUserOrOwnerOrAdminOrEmployee(customer1.Id, customer2));
        }

        [TestMethod]
        public void CorrectUserOrOwnerOrAdminOrEmployee_ShouldReturn_TrueIfLoggedInUserIsAdmin()
        {
            LoggedInAs = admin;

            Assert.IsTrue(CorrectUserOrOwnerOrAdminOrEmployee(customer1.Id, customer2));
        }

        [TestMethod]
        public void CorrectUserOrOwnerOrAdminOrEmployee_ShouldReturn_TrueIfLoggedInUserIsEmployeeAndTheUsersInTheParametersAreCustomers()
        {
            LoggedInAs = employee1;

            Assert.IsTrue(CorrectUserOrOwnerOrAdminOrEmployee(customer1.Id, customer2));
        }

        [TestMethod]
        public void CorrectUserOrOwnerOrAdminOrEmployee_ShouldReturn_FalseIfLoggedInUserIsNotTheSameAsAnyOfTheMethodParameters()
        {
            LoggedInAs = customer2;

            Assert.IsFalse(CorrectUserOrOwnerOrAdminOrEmployee(customer1.Id, customer3));
        }

        [TestMethod]
        public void CorrectUserOrOwnerOrAdminOrEmployee_ShouldFail_NoOneLoggedIn()
        {
            LoggedInAs = null;

            var ex = Assert.ThrowsException<LoginException>(()
                => CorrectUserOrOwnerOrAdminOrEmployee(customer2.Id, customer1));

            Assert.AreEqual("Not logged in!", ex.Message);
        }


        [TestMethod]
        public void CheckIfUserIsAllowedToPerformAction_ShouldReturn_TrueIfLoggedInUserIsAdminAndUserInMethodParameterIsCustomer()
        {
            LoggedInAs = admin;

            Assert.IsTrue(CheckIfUserIsAllowedToPerformAction(customer2));
        }

        [TestMethod]
        public void CheckIfUserIsAllowedToPerformAction_ShouldReturn_TrueIfLoggedInUserIsEmployeeAndUserInMethodParameterIsCustomer()
        {
            LoggedInAs = employee1;

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
            Assert.IsFalse(CheckIfUserIsAllowedToPerformAction(employee1));
            Assert.IsFalse(CheckIfUserIsAllowedToPerformAction(employeeRequester));
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
        public async Task LoginUser_ShouldFail_IfTheUserTryingToLoginIsOfUsertypeRequestToBeEmployee()
        {
            LoggedInAs = null;
            await SetupDatabaseForTestingAsync();
            SetupTestClasses();
            await CreateDummyUsersAsync();

            var result = typeof(Authentication)
                .GetMethod("LoginUser", BindingFlags.NonPublic | BindingFlags.Static);

            var ex = await Assert.ThrowsExceptionAsync<TargetInvocationException>(async ()
                => await (Task)result.Invoke(null, new object[] { employeeRequester.UserName, "aaaaaa", await userService.GetAllUsersForLoginAsync() }));


            Assert.AreEqual(ex.InnerException.GetType(), typeof(LoginException));

            Assert.AreEqual("The request to have an employee account must be approved by admin before it can be used!", ex.InnerException.Message);
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
