using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Helpers;
using AintBnB.Core.Models;
using NUnit.Framework;
using System.Reflection;
using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace Test.Unit
{
    [TestFixture]
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

        [Test]
        public void AdminChecker_ShouldReturn_TrueIfAdminIsLoggedIn()
        {
            LoggedInAs = admin;

            Assert.True(AdminChecker());
        }

        [Test]
        public void AdminChecker_ShouldReturn_FalseIfLoggedInUserIsNotAdmin()
        {
            LoggedInAs = customer1;

            Assert.False(AdminChecker());

            LoggedInAs = employee1;

            Assert.False(AdminChecker());

            LoggedInAs = employeeRequester;

            Assert.False(AdminChecker());
        }

        [Test]
        public void AdminChecker_ShouldFail_IfNoOneIsLoggedIn()
        {
            LoggedInAs = null;

            var ex = Assert.Throws<LoginException>(()
                => AdminChecker());

            Assert.AreEqual("Not logged in!", ex.Message);
        }

        [Test]
        public void EmployeeChecker_ShouldReturn_TrueIfEmployeeIsLoggedIn()
        {
            LoggedInAs = employee1;

            Assert.True(EmployeeChecker());
        }

        [Test]
        public void EmployeeChecker_ShouldReturn_FalseIfLoggedInUserIsNotEmployee()
        {
            LoggedInAs = customer1;

            Assert.False(EmployeeChecker());

            LoggedInAs = admin;

            Assert.False(EmployeeChecker());

            LoggedInAs = employeeRequester;

            Assert.False(EmployeeChecker());
        }

        [Test]
        public void EmployeeChecker_ShouldFail_IfNoOneIsLoggedIn()
        {
            LoggedInAs = null;

            var ex = Assert.Throws<LoginException>(()
                => EmployeeChecker());

            Assert.AreEqual("Not logged in!", ex.Message);
        }

        [Test]
        public void CorrectUserOrAdminOrEmployee_ShouldReturn_TrueIfLoggedInUserIsAdminOrEmployeeOrTheUserBeingChecked()
        {
            LoggedInAs = customer1;

            Assert.True(CorrectUserOrAdminOrEmployee(customer1));

            LoggedInAs = admin;

            Assert.True(CorrectUserOrAdminOrEmployee(customer1));

            LoggedInAs = employee1;

            Assert.True(CorrectUserOrAdminOrEmployee(customer1));
        }

        [Test]
        public void CorrectUserOrAdminOrEmployee_ShouldReturn_FalseIfLoggedInUserIsCustomerButUserInMethodParameterIsNotTheLoggedInCustomer()
        {
            LoggedInAs = customer1;

            Assert.False(CorrectUserOrAdminOrEmployee(customer2));
        }

        [Test]
        public void CorrectUserOrAdminOrEmployee_ShouldReturn_FalseIfLoggedInUserIsEmployeeButUserInMethodParameterIsNotTheLoggedInEmployeeOrACustomer()
        {
            LoggedInAs = employee1;

            Assert.False(CorrectUserOrAdminOrEmployee(admin));
            Assert.False(CorrectUserOrAdminOrEmployee(employee2));
            Assert.False(CorrectUserOrAdminOrEmployee(employeeRequester));
        }

        [Test]
        public void CorrectUserOrAdminOrEmployee_ShouldFail_NoOneLoggedIn()
        {
            LoggedInAs = null;

            var ex = Assert.Throws<LoginException>(()
                => CorrectUserOrAdminOrEmployee(customer1));

            Assert.AreEqual("Not logged in!", ex.Message);
        }

        [Test]
        public void HasElevatedRights_ShouldReturn_TrueIfLoggedInUserIsAdminOrEmployee()
        {
            LoggedInAs = employee1;

            Assert.True(HasElevatedRights());

            LoggedInAs = admin;

            Assert.True(HasElevatedRights());
        }

        [Test]
        public void HasElevatedRights_ShouldReturn_FalseIfLoggedInUserIsNotAdminOrEmployee()
        {
            LoggedInAs = customer1;

            Assert.False(HasElevatedRights());

            LoggedInAs = employeeRequester;

            Assert.False(HasElevatedRights());
        }

        [Test]
        public void HasElevatedRights_ShouldFail_NoOneLoggedIn()
        {
            LoggedInAs = null;

            var ex = Assert.Throws<LoginException>(()
                => HasElevatedRights());

            Assert.AreEqual("Not logged in!", ex.Message);
        }

        [Test]
        public void CorrectUserOrAdmin_ShouldReturn_TrueIfLoggedInUserIsAdminOrTheUserBeingChecked()
        {
            LoggedInAs = customer1;

            Assert.True(CorrectUserOrAdmin(customer1.Id));

            LoggedInAs = admin;

            Assert.True(CorrectUserOrAdmin(customer1.Id));
        }

        [Test]
        public void CorrectUserOrAdmin_ShouldReturn_FalseIfLoggedInUserIsEmployee()
        {
            LoggedInAs = employee1;

            Assert.False(CorrectUserOrAdmin(customer2.Id));
        }

        [Test]
        public void CorrectUserOrAdmin_ShouldReturn_FalseIfLoggedInUserIsCustomerButUserInMethodParameterIsNotTheLoggedInCustomer()
        {
            LoggedInAs = customer2;

            Assert.False(CorrectUserOrAdmin(customer1.Id));
        }

        [Test]
        public void CorrectUserOrAdmin_ShouldFail_NoOneLoggedIn()
        {
            LoggedInAs = null;

            var ex = Assert.Throws<LoginException>(()
                => CorrectUserOrAdmin(customer1.Id));

            Assert.AreEqual("Not logged in!", ex.Message);
        }

        [Test]
        public void CorrectUserOrOwnerOrAdminOrEmployee_ShouldReturn_TrueIfLoggedInUserIsTheSameAsAnyOfTheMethodParameters()
        {
            LoggedInAs = customer1;

            Assert.True(CorrectUserOrOwnerOrAdminOrEmployee(customer1.Id, customer2));

            LoggedInAs = customer2;

            Assert.True(CorrectUserOrOwnerOrAdminOrEmployee(customer1.Id, customer2));
        }

        [Test]
        public void CorrectUserOrOwnerOrAdminOrEmployee_ShouldReturn_TrueIfLoggedInUserIsAdmin()
        {
            LoggedInAs = admin;

            Assert.True(CorrectUserOrOwnerOrAdminOrEmployee(customer1.Id, customer2));
        }

        [Test]
        public void CorrectUserOrOwnerOrAdminOrEmployee_ShouldReturn_TrueIfLoggedInUserIsEmployeeAndTheUsersInTheParametersAreCustomers()
        {
            LoggedInAs = employee1;

            Assert.True(CorrectUserOrOwnerOrAdminOrEmployee(customer1.Id, customer2));
        }

        [Test]
        public void CorrectUserOrOwnerOrAdminOrEmployee_ShouldReturn_FalseIfLoggedInUserIsNotTheSameAsAnyOfTheMethodParameters()
        {
            LoggedInAs = customer2;

            Assert.False(CorrectUserOrOwnerOrAdminOrEmployee(customer1.Id, customer3));
        }

        [Test]
        public void CorrectUserOrOwnerOrAdminOrEmployee_ShouldFail_NoOneLoggedIn()
        {
            LoggedInAs = null;

            var ex = Assert.Throws<LoginException>(()
                => CorrectUserOrOwnerOrAdminOrEmployee(customer2.Id, customer1));

            Assert.AreEqual("Not logged in!", ex.Message);
        }


        [Test]
        public void CheckIfUserIsAllowedToPerformAction_ShouldReturn_TrueIfLoggedInUserIsAdminAndUserInMethodParameterIsCustomer()
        {
            LoggedInAs = admin;

            Assert.True(CheckIfUserIsAllowedToPerformAction(customer2));
        }

        [Test]
        public void CheckIfUserIsAllowedToPerformAction_ShouldReturn_TrueIfLoggedInUserIsEmployeeAndUserInMethodParameterIsCustomer()
        {
            LoggedInAs = employee1;

            Assert.True(CheckIfUserIsAllowedToPerformAction(customer2));
        }

        [Test]
        public void CheckIfUserIsAllowedToPerformAction_ShouldReturn_TrueIfLoggedInUserIsCustomerAndIsTheSameAsTheUserInTheMethodParameter()
        {
            LoggedInAs = customer2;

            Assert.True(CheckIfUserIsAllowedToPerformAction(customer2));
        }

        [Test]
        public void CheckIfUserIsAllowedToPerformAction_ShouldReturn_FalseIfTheUserInTheMethodParameterIsNotCustomer()
        {
            LoggedInAs = admin;

            Assert.False(CheckIfUserIsAllowedToPerformAction(admin));
            Assert.False(CheckIfUserIsAllowedToPerformAction(employee1));
            Assert.False(CheckIfUserIsAllowedToPerformAction(employeeRequester));
        }

        [Test]
        public void CheckIfUserIsAllowedToPerformAction_ShouldFail_NoOneLoggedIn()
        {
            LoggedInAs = null;

            var ex = Assert.Throws<LoginException>(()
                => CheckIfUserIsAllowedToPerformAction(customer1));

            Assert.AreEqual("Not logged in!", ex.Message);
        }



        [Test]
        public void LoginUser_ShouldSucceed_IfTheUserLoggingInHasEnteredCorrectUsernameAndPassword()
        {
            LoggedInAs = null;
            SetupDatabaseForTesting();
            SetupTestClasses();
            CreateDummyUsers();

            var result = typeof(Authentication)
                .GetMethod("LoginUser", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.DoesNotThrow(() 
                => result.Invoke(null, new object[] { userCustomer1.UserName, "aaaaaa", userService.GetAllUsersForLogin() }));
        }

        [Test]
        public void LoginUser_ShouldFail_IfTheUsernameIsWrong()
        {
            LoggedInAs = null;
            SetupDatabaseForTesting();
            SetupTestClasses();
            CreateDummyUsers();

            var result = typeof(Authentication)
                .GetMethod("LoginUser", BindingFlags.NonPublic | BindingFlags.Static);

            var ex = Assert.Throws<TargetInvocationException>(()
                => result.Invoke(null, new object[] { "ssssssssssssssss", "aaaaaa", userService.GetAllUsersForLogin() }));


            Assert.AreEqual(ex.InnerException.GetType(), typeof(LoginException));

            Assert.AreEqual("Username and/or password not correct!", ex.InnerException.Message);
        }

        [Test]
        public void LoginUser_ShouldFail_IfThePasswordIsWrong()
        {
            LoggedInAs = null;
            SetupDatabaseForTesting();
            SetupTestClasses();
            CreateDummyUsers();

            var result = typeof(Authentication)
                .GetMethod("LoginUser", BindingFlags.NonPublic | BindingFlags.Static);

            var ex = Assert.Throws<TargetInvocationException>(()
                => result.Invoke(null, new object[] { userCustomer1.UserName, "blablablabla", userService.GetAllUsersForLogin() }));


            Assert.AreEqual(ex.InnerException.GetType(), typeof(LoginException));

            Assert.AreEqual("Username and/or password not correct!", ex.InnerException.Message);
        }

        [Test]
        public void LoginUser_ShouldFail_IfTheUserTryingToLoginIsOfUsertypeRequestToBeEmployee()
        {
            LoggedInAs = null;
            SetupDatabaseForTesting();
            SetupTestClasses();
            CreateDummyUsers();

            var result = typeof(Authentication)
                .GetMethod("LoginUser", BindingFlags.NonPublic | BindingFlags.Static);

            var ex = Assert.Throws<TargetInvocationException>(()
                => result.Invoke(null, new object[] { employeeRequester.UserName, "aaaaaa", userService.GetAllUsersForLogin() }));


            Assert.AreEqual(ex.InnerException.GetType(), typeof(LoginException));

            Assert.AreEqual("The request to have an employee account must be approved by admin before it can be used!", ex.InnerException.Message);
        }

        [Test]
        public void TryToLogin_ShouldFail_IfAlreadyLoggedIn()
        {
            SetupDatabaseForTesting();
            SetupTestClasses();
            CreateDummyUsers();

            LoggedInAs = userCustomer1;

            var ex = Assert.Throws<AlreadyLoggedInException>(()
                => TryToLogin(userCustomer1.UserName, "blablablabla", userService.GetAllUsersForLogin()));

            Assert.AreEqual("Already logged in!", ex.Message);
        }
    }
}
