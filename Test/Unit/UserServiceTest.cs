using AintBnB.Core.Models;
using NUnit.Framework;
using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Imp;
using System.Reflection;
using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace Test.Unit
{
    [TestFixture]
    public class UserServiceTest : TestBase
    {

        [SetUp]
        public void SetUp()
        {
            SetupDatabaseForTesting();
            SetupTestClasses();
        }

        [TearDown]
        public void TearDown()
        {
            Dispose();
        }

        [Test]
        public void GetAllUsersForLogin_ShouldReturn_ListOfAllUsers_WhenNoOneLoggedIn()
        {
            CreateDummyUsers();

            Logout();

            Assert.AreEqual(6, userService.GetAllUsersForLogin().Count);
            Assert.AreEqual(UserTypes.Admin, userService.GetAllUsersForLogin()[0].UserType);
        }

        [Test]
        public void GetAllUsersForLogin_ShoudFail_WhenLoggingInWithoutAnyRegisteredUsers()
        {
            Logout();

            var ex = Assert.Throws<NoneFoundInDatabaseTableException>(()
                => userService.GetAllUsersForLogin());

            Assert.AreEqual(ex.Message, "No users found!");
        }

        [Test]
        public void GetAllUsersForLogin_ShoudFail_WhenAlreadyLoggedIn()
        {
            CreateDummyUsers();

            LoggedInAs = userAdmin;
            
            var ex = Assert.Throws<AlreadyLoggedInException>(()
                => userService.GetAllUsersForLogin());

            Assert.AreEqual(ex.Message, "Already logged in!");
        }

        [Test]
        public void CreateUser_ShouldReturn_NewlyCreatedUser()
        {
            User first = userService.CreateUser("admin", "aaaaaa", "adminfirstname", "adminlastname", UserTypes.Employee);
            User second = userService.CreateUser("empreq", "aaaaaa", "empreqfirstname", "empreqlastname", UserTypes.RequestToBeEmployee);
            User third = userService.CreateUser("cust1", "aaaaaa", "customerfirstname", "customerlastname", UserTypes.Admin);
            User fourth = userService.CreateUser("cust2", "aaaaaa", "anothercustomerfirstname", "anothercustomerlastname", UserTypes.Employee);

            Assert.AreEqual(1, first.Id);
            Assert.AreEqual("admin", first.UserName);
            Assert.AreEqual(UserTypes.RequestToBeEmployee, second.UserType);
            Assert.AreEqual("empreq", second.UserName);
            Assert.AreEqual(3, third.Id);
            Assert.AreEqual("cust1", third.UserName);
            Assert.AreEqual(4, fourth.Id);
            Assert.AreEqual("cust2", fourth.UserName);
        }

        [Test]
        public void IsUserNameFree_ShouldFail_IfUsernameAlreadyExists()
        {
            CreateDummyUsers();

            var result = typeof(UserService)
                            .GetMethod("IsUserNameFree", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.Throws<TargetInvocationException>(()
                => result.Invoke(userService, new object[] { "admin" }));

            Assert.AreEqual(ex.InnerException.GetType(), typeof(ParameterException));

            Assert.AreEqual(ex.InnerException.Message, "Username already taken!");
        }

        [Test]
        public void UserTypeCheck_ShouldSetUserTypeToAdmin_IfFirstCreatedUser()
        {
            User dummy = new User { UserType = UserTypes.Employee };
            User dummy2 = new User { UserType = UserTypes.RequestToBeEmployee };
            User dummy3 = new User { UserType = UserTypes.Customer };

            var result = typeof(UserService)
                            .GetMethod("UserTypeCheck", BindingFlags.NonPublic | BindingFlags.Instance);
            result.Invoke(userService, new object[] { dummy.UserType, dummy });
            result.Invoke(userService, new object[] { dummy2.UserType, dummy2 });
            result.Invoke(userService, new object[] { dummy3.UserType, dummy3 });

            Assert.AreEqual(UserTypes.Admin, dummy.UserType);
            Assert.AreEqual(UserTypes.Admin, dummy2.UserType);
            Assert.AreEqual(UserTypes.Admin, dummy3.UserType);
        }

        [Test]
        public void UserTypeCheck_ShouldSetUserTypeToCustomer_IfNotRequestToBeEmployeeOrFirstCreatedUser()
        {
            CreateDummyUsers();

            User dummy = new User { UserType = UserTypes.Employee };

            var result = typeof(UserService)
                            .GetMethod("UserTypeCheck", BindingFlags.NonPublic | BindingFlags.Instance);
            result.Invoke(userService, new object[] { dummy.UserType, dummy });

            Assert.AreEqual(UserTypes.Customer, dummy.UserType);
        }

        [Test]
        public void UserTypeCheck_ShouldSetUserTypeToRequestToBeEmployee_IfUserHasThatUserTypeAndNotFirstCreatedUser()
        {
            CreateDummyUsers();

            User dummy = new User { UserType = UserTypes.RequestToBeEmployee };

            var result = typeof(UserService)
                            .GetMethod("UserTypeCheck", BindingFlags.NonPublic | BindingFlags.Instance);
            result.Invoke(userService, new object[] { dummy.UserType, dummy });

            Assert.AreEqual(UserTypes.RequestToBeEmployee, dummy.UserType);
        }

        [Test]
        [TestCase(1, "newfirstone", "newlastone")]
        [TestCase(2, "newfirsttwo", "newlasttwo")]
        [TestCase(3, "newfirstthree", "newlastthree")]
        [TestCase(4, "newfirstfour", "newlastfour")]
        public void UpdateUser_ShouldSucceed_WhenUsersChangeFirstAndLastName(int id, string newFirstname, string newLastname)
        {
            CreateDummyUsers();

            LoggedInAs = unitOfWork.UserRepository.Read(id);

            Assert.AreNotEqual(userService.GetUser(id).FirstName, newFirstname);
            Assert.AreNotEqual(userService.GetUser(id).LastName, newLastname);

            unitOfWork.UserRepository.Read(id).FirstName = newFirstname;
            unitOfWork.UserRepository.Read(id).LastName = newLastname;

            userService.UpdateUser(id, unitOfWork.UserRepository.Read(id));

            Assert.AreEqual(userService.GetUser(id).FirstName, newFirstname);
            Assert.AreEqual(userService.GetUser(id).LastName, newLastname);
        }

        [Test]
        public void UpdateUser_ShouldSucceed_WhenEmployeeUpdatesCustomerAccounts()
        {
            CreateDummyUsers();

            LoggedInAs = userEmployee1;

            string newFirstname = "blahb";
            string newLastname = "lablah";

            Assert.AreNotEqual(userCustomer1.FirstName, newFirstname);
            Assert.AreNotEqual(userCustomer1.LastName, newLastname);

            userCustomer1.FirstName = newFirstname;
            userCustomer1.LastName = newLastname;

            userService.UpdateUser(userCustomer1.Id, userCustomer1);

            Assert.AreEqual(userCustomer1.FirstName, newFirstname);
            Assert.AreEqual(userCustomer1.LastName, newLastname);
        }

        [Test]
        public void Admin_Can_Grant_Employee_Account_Request()
        {
            CreateDummyUsers();

            LoggedInAs = userAdmin;

            userRequestToBecomeEmployee.UserType = UserTypes.Employee;
            userRequestToBecomeEmployee2.UserType = UserTypes.Employee;

            userService.UpdateUser(userRequestToBecomeEmployee.Id, userRequestToBecomeEmployee);
            userService.UpdateUser(userRequestToBecomeEmployee2.Id, userRequestToBecomeEmployee2);

            Assert.AreEqual(UserTypes.Employee, userRequestToBecomeEmployee.UserType);
            Assert.AreEqual(UserTypes.Employee, userRequestToBecomeEmployee2.UserType);
        }

        [Test]
        [TestCase(1, "<'newpassone'>", "<'newpassone'>")]
        [TestCase(2, "newpasstwo", "newpasstwo")]
        [TestCase(3, "newpassthree", "newpassthree")]
        [TestCase(4, "newpassfour", "newpassfour")]
        public void ChangePassword_ShouldSucceed_WhenUserEntersCorrectOld_And_CanConfirmNewPassword(int id, string newPass, string newPassConfirm)
        {
            CreateDummyUsers();

            LoggedInAs = unitOfWork.UserRepository.Read(id);

            Assert.IsFalse(UnHashPassword(newPass, LoggedInAs.Password));

            userService.ChangePassword("aaaaaa", id, newPass, newPassConfirm);

            Assert.IsTrue(UnHashPassword(newPass, LoggedInAs.Password));
        }

        [Test]
        [TestCase(1, "<'newpassone'>", "<'newpassone'>")]
        [TestCase(2, "aaaaaa", "aaaaaa")]
        [TestCase(3, "aaaaaabb", "aaaaaab")]
        public void ChangePassword_ShouldFailWhen_OldPasswordWrong_NewPasswordConfirmationFails_UnchangedNewAndOld(int id, string newPass, string newPassConfirmed)
        {
            CreateDummyUsers();

            LoggedInAs = unitOfWork.UserRepository.Read(id);

            if (id == 1)
            {
                var ex = Assert.Throws<PasswordChangeException>(()
                    => userService.ChangePassword("aaaaaab", id, newPass, newPassConfirmed));

                Assert.AreEqual(ex.Message, "The old passwords don't match!");
            }
            else if (id == 2)
            {
                var ex = Assert.Throws<PasswordChangeException>(()
                    => userService.ChangePassword("aaaaaa", id, newPass, newPassConfirmed));

                Assert.AreEqual(ex.Message, "The new and old password must be different!");
            }
            else if (id == 3)
            {
                var ex = Assert.Throws<PasswordChangeException>(()
                    => userService.ChangePassword("aaaaaa", id, newPass, newPassConfirmed));

                Assert.AreEqual(ex.Message, "The new passwords don't match!");
            }
        }


        [Test]
        [TestCase(2, "newpasstwo", "aaaaaa")]
        [TestCase(3, "newpassthree", "aaaaaa")]
        public void ChangePassword_ShouldFail_WhenNotDoneByAccountOwner(int id, string oldPass, string newPass)
        {
            CreateDummyUsers();

            LoggedInAs = unitOfWork.UserRepository.Read(id-1);

            var ex = Assert.Throws<AccessException>(()
                => userService.ChangePassword(oldPass, id, newPass, newPass));

            Assert.AreEqual(ex.Message, "Only the owner of the account can change their password!");
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void GetUser_ShouldReturn_CorrectUser(int id)
        {
            CreateDummyUsers();

            LoggedInAs = unitOfWork.UserRepository.Read(id);

            User user = userService.GetUser(id);

            Assert.AreEqual(id, user.Id);
        }

        [Test]
        public void GetUser_ShouldFail_IfCustomerTriesToGetAnotherUserAccount()
        {
            CreateDummyUsers();

            LoggedInAs = userCustomer1;

            var exc = Assert.Throws<AccessException>(()
                => userService.GetUser(6));

            Assert.AreEqual(exc.Message, "Restricted access!");
        }

        [Test]
        public void GetUser_ShouldFail_IfNoUsersWithTheIdExists()
        {
            CreateDummyUsers();

            LoggedInAs = userAdmin;

            var ex = Assert.Throws<IdNotFoundException>(()
                => userService.GetUser(600));

            Assert.AreEqual("User with ID 600 not found!", ex.Message);
        }

        [Test]
        public void Only_Admin_Can_View_Admin_Account()
        {
            CreateDummyUsers();

            LoggedInAs = userAdmin;

            Assert.AreEqual(LoggedInAs.Id, userService.GetUser(1).Id);

            LoggedInAs = userEmployee1;

            var ex = Assert.Throws<AccessException>(()
                => userService.GetUser(1));

            Assert.AreEqual(ex.Message, "Restricted access!");

            LoggedInAs = userRequestToBecomeEmployee;

            ex = Assert.Throws<AccessException>(()
                => userService.GetUser(1));

            Assert.AreEqual(ex.Message, "Restricted access!");

            LoggedInAs = userCustomer1;

            ex = Assert.Throws<AccessException>(()
                => userService.GetUser(1));

            Assert.AreEqual(ex.Message, "Restricted access!");
        }

        [Test]
        public void GetAllUsers_ShouldReturn_AllUsersWhenAdmin()
        {
            CreateDummyUsers();

            LoggedInAs = userAdmin;

            Assert.AreEqual(6, userService.GetAllUsers().Count);
        }

        [Test]
        public void GetAllUsers_ShouldReturn_AllCustomers_And_TheirOwnAccount_WhenEmployee()
        {
            CreateDummyUsers();

            LoggedInAs = userEmployee1;

            Assert.AreEqual(3, userService.GetAllUsers().Count);
            Assert.AreEqual(2, userService.GetAllUsers()[0].Id);
            Assert.AreEqual(UserTypes.Customer, userService.GetAllUsers()[1].UserType);
            Assert.AreEqual(UserTypes.Customer, userService.GetAllUsers()[2].UserType);
        }

        [Test]
        public void GetAllUsers_ShouldFail_WhenCustomer()
        {
            CreateDummyUsers();

            LoggedInAs = userCustomer1;

            var ex = Assert.Throws<AccessException>(()
                => userService.GetAllUsers());

            Assert.AreEqual(ex.Message, "Restricted access!");
        }

        [Test]
        public void GetAllEmployeeRequests_ShouldReturn_AllEmployeeRequestsIfAdmin()
        {
            CreateDummyUsers();

            LoggedInAs = userAdmin;

            Assert.AreEqual(2, userService.GetAllEmployeeRequests().Count);
        }

        [Test]
        public void GetAllEmployeeRequests_ShouldFail_IfNotAdmin()
        {
            CreateDummyUsers();

            LoggedInAs = userEmployee1;

            var ex = Assert.Throws<AccessException>(()
                => userService.GetAllEmployeeRequests());

            Assert.AreEqual(ex.Message, "Admin only!");
        }

        [Test]
        public void Validate_ShouldFail_If_UsernameEmpty_FirstAndLastnames_AreNotOnlyLetters_WithOneDash_OrSpace_BetweenNames()
        {
            var ex = Assert.Throws<ParameterException>(()
                => userService.ValidateUser("", "ss", "dd"));

            Assert.AreEqual(ex.Message, "UserName cannot be empty!");

            ex = Assert.Throws<ParameterException>(()
                => userService.ValidateUser("s", "ss9", "dd"));

            Assert.AreEqual(ex.Message, "FirstName cannot be containing any other than letters and one space or dash between names!");

            ex = Assert.Throws<ParameterException>(()
                => userService.ValidateUser("s", "ss", "dd-"));

            Assert.AreEqual(ex.Message, "LastName cannot be containing any other than letters and one space or dash between names!");
        }
    }
}