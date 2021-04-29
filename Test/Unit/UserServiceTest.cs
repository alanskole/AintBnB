using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Imp;
using AintBnB.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Threading.Tasks;

using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace Test.Unit
{
    [TestClass]
    public class UserServiceTest : TestBase
    {

        [TestInitialize]
        public async Task SetUp()
        {
            await SetupDatabaseForTestingAsync();
            SetupTestClasses();
        }

        [TestCleanup]
        public async Task TearDown()
        {
            await DisposeAsync();
        }

        [TestMethod]
        public async Task GetAllUsersForLoginAsync_ShouldReturn_ListOfAllUsers_WhenNoOneLoggedIn()
        {
            await CreateDummyUsersAsync();

            Logout();
            var all = await userService.GetAllUsersForLoginAsync();
            Assert.AreEqual(7, all.Count);
            Assert.AreEqual(UserTypes.Admin, all[0].UserType);
        }

        [TestMethod]
        public async Task GetAllUsersForLoginAsync_ShoudFail_WhenLoggingInWithoutAnyRegisteredUsersAsync()
        {
            Logout();

            var ex = await Assert.ThrowsExceptionAsync<NoneFoundInDatabaseTableException>(async ()
                => await userService.GetAllUsersForLoginAsync());

            Assert.AreEqual(ex.Message, "No users found!");
        }

        [TestMethod]
        public async Task GetAllUsersForLoginAsync_ShoudFail_WhenAlreadyLoggedIn()
        {
            await CreateDummyUsersAsync();

            LoggedInAs = userAdmin;

            var ex = await Assert.ThrowsExceptionAsync<AlreadyLoggedInException>(async ()
                => await userService.GetAllUsersForLoginAsync());

            Assert.AreEqual(ex.Message, "Already logged in!");
        }

        [TestMethod]
        public async Task CreateUserAsync_ShouldReturn_NewlyCreatedUser()
        {
            var first = await userService.CreateUserAsync("admin", "aaaaaa", "adminfirstname", "adminlastname", UserTypes.Employee);
            var second = await userService.CreateUserAsync("empreq", "aaaaaa", "empreqfirstname", "empreqlastname", UserTypes.RequestToBeEmployee);
            var third = await userService.CreateUserAsync("cust1", "aaaaaa", "customerfirstname", "customerlastname", UserTypes.Admin);
            var fourth = await userService.CreateUserAsync("cust2", "aaaaaa", "anothercustomerfirstname", "anothercustomerlastname", UserTypes.Employee);

            Assert.AreEqual(1, first.Id);
            Assert.AreEqual("admin", first.UserName);
            Assert.AreEqual(UserTypes.RequestToBeEmployee, second.UserType);
            Assert.AreEqual("empreq", second.UserName);
            Assert.AreEqual(3, third.Id);
            Assert.AreEqual("cust1", third.UserName);
            Assert.AreEqual(4, fourth.Id);
            Assert.AreEqual("cust2", fourth.UserName);
        }

        [TestMethod]
        public async Task IsUserNameFreeAsync_ShouldFail_IfUsernameAlreadyExists()
        {
            await CreateDummyUsersAsync();

            var result = typeof(UserService)
                .GetMethod("IsUserNameFreeAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = await Assert.ThrowsExceptionAsync<ParameterException>(async ()
                => await (Task)result.Invoke(userService, new object[] { "admin" }));

            Assert.AreEqual(ex.Message, "Username already taken!");
        }

        [TestMethod]
        public async Task UserTypeCheckAsync_ShouldSetUserTypeToAdmin_IfFirstCreatedUser()
        {
            var dummy = new User { UserType = UserTypes.Employee };
            var dummy2 = new User { UserType = UserTypes.RequestToBeEmployee };
            var dummy3 = new User { UserType = UserTypes.Customer };

            var result = typeof(UserService)
                .GetMethod("UserTypeCheckAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            await (Task)result.Invoke(userService, new object[] { dummy.UserType, dummy });
            await (Task)result.Invoke(userService, new object[] { dummy2.UserType, dummy2 });
            await (Task)result.Invoke(userService, new object[] { dummy3.UserType, dummy3 });

            Assert.AreEqual(UserTypes.Admin, dummy.UserType);
            Assert.AreEqual(UserTypes.Admin, dummy2.UserType);
            Assert.AreEqual(UserTypes.Admin, dummy3.UserType);
        }

        [TestMethod]
        public async Task UserTypeCheckAsync_ShouldSetUserTypeToCustomer_IfNotRequestToBeEmployeeOrFirstCreatedUser()
        {
            await CreateDummyUsersAsync();

            var dummy = new User { UserType = UserTypes.Employee };

            var result = typeof(UserService)
                .GetMethod("UserTypeCheckAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            await (Task)result.Invoke(userService, new object[] { dummy.UserType, dummy });

            Assert.AreEqual(UserTypes.Customer, dummy.UserType);
        }

        [TestMethod]
        public async Task UserTypeCheckAsync_ShouldSetUserTypeToRequestToBeEmployee_IfUserHasThatUserTypeAndNotFirstCreatedUser()
        {
            await CreateDummyUsersAsync();

            User dummy = new User { UserType = UserTypes.RequestToBeEmployee };

            var result = typeof(UserService)
                .GetMethod("UserTypeCheckAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            await (Task)result.Invoke(userService, new object[] { dummy.UserType, dummy });

            Assert.AreEqual(UserTypes.RequestToBeEmployee, dummy.UserType);
        }

        [DataRow(1, "newfirstone", "newlastone")]
        [DataRow(2, "newfirsttwo", "newlasttwo")]
        [DataRow(3, "newfirstthree", "newlastthree")]
        [DataRow(4, "newfirstfour", "newlastfour")]
        [DataTestMethod]
        public async Task UpdateUserAsync_ShouldSucceed_WhenUsersChangeFirstAndLastName(int id, string newFirstname, string newLastname)
        {
            await CreateDummyUsersAsync();

            LoggedInAs = await unitOfWork.UserRepository.ReadAsync(id);

            var user = await userService.GetUserAsync(id);

            Assert.AreNotEqual(user.FirstName, newFirstname);
            Assert.AreNotEqual(user.LastName, newLastname);

            user.FirstName = newFirstname;
            user.LastName = newLastname;

            await userService.UpdateUserAsync(id, await unitOfWork.UserRepository.ReadAsync(id));

            user = await userService.GetUserAsync(id);

            Assert.AreEqual(user.FirstName, newFirstname);
            Assert.AreEqual(user.LastName, newLastname);
        }

        [TestMethod]
        public async Task UpdateUserAsync_ShouldSucceed_WhenEmployeeUpdatesCustomerAccounts()
        {
            await CreateDummyUsersAsync();

            LoggedInAs = userEmployee1;

            string newFirstname = "blahb";
            string newLastname = "lablah";

            Assert.AreNotEqual(userCustomer1.FirstName, newFirstname);
            Assert.AreNotEqual(userCustomer1.LastName, newLastname);

            userCustomer1.FirstName = newFirstname;
            userCustomer1.LastName = newLastname;

            await userService.UpdateUserAsync(userCustomer1.Id, userCustomer1);

            Assert.AreEqual(userCustomer1.FirstName, newFirstname);
            Assert.AreEqual(userCustomer1.LastName, newLastname);
        }

        [TestMethod]
        public async Task Admin_Can_Grant_Employee_Account_Request()
        {
            await CreateDummyUsersAsync();

            LoggedInAs = userAdmin;

            userRequestToBecomeEmployee.UserType = UserTypes.Employee;
            userRequestToBecomeEmployee2.UserType = UserTypes.Employee;

            await userService.UpdateUserAsync(userRequestToBecomeEmployee.Id, userRequestToBecomeEmployee);
            await userService.UpdateUserAsync(userRequestToBecomeEmployee2.Id, userRequestToBecomeEmployee2);

            Assert.AreEqual(UserTypes.Employee, userRequestToBecomeEmployee.UserType);
            Assert.AreEqual(UserTypes.Employee, userRequestToBecomeEmployee2.UserType);
        }

        [DataRow(1, "<'newpassone'>", "<'newpassone'>")]
        [DataRow(2, "newpasstwo", "newpasstwo")]
        [DataRow(3, "newpassthree", "newpassthree")]
        [DataRow(4, "newpassfour", "newpassfour")]
        [DataTestMethod]
        public async Task ChangePasswordAsync_ShouldSucceed_WhenUserEntersCorrectOld_And_CanConfirmNewPassword(int id, string newPass, string newPassConfirm)
        {
            await CreateDummyUsersAsync();

            LoggedInAs = await unitOfWork.UserRepository.ReadAsync(id);

            Assert.IsFalse(UnHashPassword(newPass, LoggedInAs.Password));

            await userService.ChangePasswordAsync("aaaaaa", id, newPass, newPassConfirm);

            Assert.IsTrue(UnHashPassword(newPass, LoggedInAs.Password));
        }

        [DataRow(1, "<'newpassone'>", "<'newpassone'>")]
        [DataRow(2, "aaaaaa", "aaaaaa")]
        [DataRow(3, "aaaaaabb", "aaaaaab")]
        [DataTestMethod]
        public async Task ChangePasswordAsync_ShouldFailWhen_OldPasswordWrong_NewPasswordConfirmationFails_UnchangedNewAndOld(int id, string newPass, string newPassConfirmed)
        {
            await CreateDummyUsersAsync();

            LoggedInAs = await unitOfWork.UserRepository.ReadAsync(id);

            if (id == 1)
            {
                var ex = await Assert.ThrowsExceptionAsync<PasswordChangeException>(async ()
                    => await userService.ChangePasswordAsync("aaaaaab", id, newPass, newPassConfirmed));

                Assert.AreEqual(ex.Message, "The old passwords don't match!");
            }
            else if (id == 2)
            {
                var ex = await Assert.ThrowsExceptionAsync<PasswordChangeException>(async ()
                    => await userService.ChangePasswordAsync("aaaaaa", id, newPass, newPassConfirmed));

                Assert.AreEqual(ex.Message, "The new and old password must be different!");
            }
            else if (id == 3)
            {
                var ex = await Assert.ThrowsExceptionAsync<PasswordChangeException>(async ()
                    => await userService.ChangePasswordAsync("aaaaaa", id, newPass, newPassConfirmed));

                Assert.AreEqual(ex.Message, "The new passwords don't match!");
            }
        }


        [DataRow(2, "newpasstwo", "aaaaaa")]
        [DataRow(3, "newpassthree", "aaaaaa")]
        [DataTestMethod]
        public async Task ChangePasswordAsync_ShouldFail_WhenNotDoneByAccountOwner(int id, string oldPass, string newPass)
        {
            await CreateDummyUsersAsync();

            LoggedInAs = await unitOfWork.UserRepository.ReadAsync(id - 1);

            var ex = await Assert.ThrowsExceptionAsync<AccessException>(async ()
                => await userService.ChangePasswordAsync(oldPass, id, newPass, newPass));

            Assert.AreEqual(ex.Message, "Only the owner of the account can change their password!");
        }

        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataTestMethod]
        public async Task GetUserAsync_ShouldReturn_CorrectUser(int id)
        {
            await CreateDummyUsersAsync();

            LoggedInAs = await unitOfWork.UserRepository.ReadAsync(id);

            User user = await userService.GetUserAsync(id);

            Assert.AreEqual(id, user.Id);
        }

        [TestMethod]
        public async Task GetUserAsync_ShouldFail_IfCustomerTriesToGetAnotherUserAccount()
        {
            await CreateDummyUsersAsync();

            LoggedInAs = userCustomer1;

            var exc = await Assert.ThrowsExceptionAsync<AccessException>(async ()
                => await userService.GetUserAsync(6));

            Assert.AreEqual(exc.Message, "Restricted access!");
        }

        [TestMethod]
        public async Task GetUserAsync_ShouldFail_IfNoUsersWithTheIdExists()
        {
            await CreateDummyUsersAsync();

            LoggedInAs = userAdmin;

            var ex = await Assert.ThrowsExceptionAsync<IdNotFoundException>(async ()
                => await userService.GetUserAsync(600));

            Assert.AreEqual("User with ID 600 not found!", ex.Message);
        }

        [TestMethod]
        public async Task Only_Admin_Can_View_Admin_Account()
        {
            await CreateDummyUsersAsync();

            LoggedInAs = userAdmin;

            await userService.GetUserAsync(1);

            LoggedInAs = userEmployee1;

            var ex = await Assert.ThrowsExceptionAsync<AccessException>(async ()
                => await userService.GetUserAsync(1));

            Assert.AreEqual(ex.Message, "Restricted access!");

            LoggedInAs = userRequestToBecomeEmployee;

            ex = await Assert.ThrowsExceptionAsync<AccessException>(async ()
                => await userService.GetUserAsync(1));

            Assert.AreEqual(ex.Message, "Restricted access!");

            LoggedInAs = userCustomer1;

            ex = await Assert.ThrowsExceptionAsync<AccessException>(async ()
                => await userService.GetUserAsync(1));

            Assert.AreEqual(ex.Message, "Restricted access!");
        }

        [TestMethod]
        public async Task GetAllUsersAsync_ShouldReturn_AllUsersWhenAdmin()
        {
            await CreateDummyUsersAsync();

            LoggedInAs = userAdmin;

            var all = await userService.GetAllUsersAsync();

            Assert.AreEqual(7, all.Count);
        }

        [TestMethod]
        public async Task GetAllUsersAsync_ShouldReturn_AllCustomers_And_TheirOwnAccount_WhenEmployee()
        {
            await CreateDummyUsersAsync();

            LoggedInAs = userEmployee1;

            var all = await userService.GetAllUsersAsync();

            Assert.AreEqual(4, all.Count);
            Assert.AreEqual(2, all[0].Id);
            Assert.AreEqual(UserTypes.Customer, all[1].UserType);
            Assert.AreEqual(UserTypes.Customer, all[2].UserType);
        }

        [TestMethod]
        public async Task GetAllUsersAsync_ShouldFail_WhenCustomer()
        {
            await CreateDummyUsersAsync();

            LoggedInAs = userCustomer1;

            var ex = await Assert.ThrowsExceptionAsync<AccessException>(async ()
                => await userService.GetAllUsersAsync());

            Assert.AreEqual(ex.Message, "Restricted access!");
        }

        [TestMethod]
        public async Task GetAllEmployeeRequestsAsync_ShouldReturn_AllEmployeeRequestsIfAdmin()
        {
            await CreateDummyUsersAsync();

            LoggedInAs = userAdmin;

            var all = await userService.GetAllEmployeeRequestsAsync();
            Assert.AreEqual(2, all.Count);
        }

        [TestMethod]
        public async Task GetAllEmployeeRequestsAsync_ShouldFail_IfNotAdmin()
        {
            await CreateDummyUsersAsync();

            LoggedInAs = userEmployee1;

            var ex = await Assert.ThrowsExceptionAsync<AccessException>(async ()
                => await userService.GetAllEmployeeRequestsAsync());

            Assert.AreEqual(ex.Message, "Admin only!");
        }

        [TestMethod]
        public void ValidateUser_ShouldFail_If_UsernameEmpty_FirstAndLastnames_AreNotOnlyLetters_WithOneDash_OrSpace_BetweenNames()
        {
            var ex = Assert.ThrowsException<ParameterException>(()
                => userService.ValidateUser("", "ss", "dd"));

            Assert.AreEqual(ex.Message, "UserName cannot be empty!");

            ex = Assert.ThrowsException<ParameterException>(()
                => userService.ValidateUser("s", "ss9", "dd"));

            Assert.AreEqual(ex.Message, "FirstName cannot be containing any other than letters and one space or dash between names!");

            ex = Assert.ThrowsException<ParameterException>(()
                => userService.ValidateUser("s", "ss", "dd-"));

            Assert.AreEqual(ex.Message, "LastName cannot be containing any other than letters and one space or dash between names!");
        }
    }
}