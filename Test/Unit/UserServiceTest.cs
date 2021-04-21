using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Imp;
using AintBnB.Core.Models;
using NUnit.Framework;
using System.Reflection;
using System.Threading.Tasks;

using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace Test.Unit
{
    [TestFixture]
    public class UserServiceTest : TestBase
    {

        [SetUp]
        public async Task SetUp()
        {
            await SetupDatabaseForTesting();
            SetupTestClasses();
        }

        [TearDown]
        public async Task TearDown()
        {
            await DisposeAsync();
        }

        [Test]
        public async Task GetAllUsersForLogin_ShouldReturn_ListOfAllUsers_WhenNoOneLoggedIn()
        {
            await CreateDummyUsers();

            Logout();
            var all = await userService.GetAllUsersForLoginAsync();
            Assert.AreEqual(6, all.Count);
            Assert.AreEqual(UserTypes.Admin, all[0].UserType);
        }

        [Test]
        public void GetAllUsersForLogin_ShoudFail_WhenLoggingInWithoutAnyRegisteredUsers()
        {
            Logout();

            var ex = Assert.ThrowsAsync<NoneFoundInDatabaseTableException>(async ()
                => await userService.GetAllUsersForLoginAsync());

            Assert.AreEqual(ex.Message, "No users found!");
        }

        [Test]
        public async Task GetAllUsersForLogin_ShoudFail_WhenAlreadyLoggedIn()
        {
            await CreateDummyUsers();

            LoggedInAs = userAdmin;

            var ex = Assert.ThrowsAsync<AlreadyLoggedInException>(async ()
                => await userService.GetAllUsersForLoginAsync());

            Assert.AreEqual(ex.Message, "Already logged in!");
        }

        [Test]
        public async Task CreateUser_ShouldReturn_NewlyCreatedUser()
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

        [Test]
        public async Task IsUserNameFree_ShouldFail_IfUsernameAlreadyExists()
        {
            await CreateDummyUsers();

            var result = typeof(UserService)
                .GetMethod("IsUserNameFreeAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await (Task)result.Invoke(userService, new object[] { "admin" }));

            Assert.AreEqual(ex.Message, "Username already taken!");
        }

        [Test]
        public void UserTypeCheck_ShouldSetUserTypeToAdmin_IfFirstCreatedUser()
        {
            var dummy = new User { UserType = UserTypes.Employee };
            var dummy2 = new User { UserType = UserTypes.RequestToBeEmployee };
            var dummy3 = new User { UserType = UserTypes.Customer };

            var result = typeof(UserService)
                            .GetMethod("UserTypeCheckAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            result.Invoke(userService, new object[] { dummy.UserType, dummy });
            result.Invoke(userService, new object[] { dummy2.UserType, dummy2 });
            result.Invoke(userService, new object[] { dummy3.UserType, dummy3 });

            Assert.AreEqual(UserTypes.Admin, dummy.UserType);
            Assert.AreEqual(UserTypes.Admin, dummy2.UserType);
            Assert.AreEqual(UserTypes.Admin, dummy3.UserType);
        }

        [Test]
        public async Task UserTypeCheck_ShouldSetUserTypeToCustomer_IfNotRequestToBeEmployeeOrFirstCreatedUser()
        {
            await CreateDummyUsers();

            var dummy = new User { UserType = UserTypes.Employee };

            var result = typeof(UserService)
                            .GetMethod("UserTypeCheckAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            result.Invoke(userService, new object[] { dummy.UserType, dummy });

            Assert.AreEqual(UserTypes.Customer, dummy.UserType);
        }

        [Test]
        public async Task UserTypeCheck_ShouldSetUserTypeToRequestToBeEmployee_IfUserHasThatUserTypeAndNotFirstCreatedUser()
        {
            await CreateDummyUsers();

            User dummy = new User { UserType = UserTypes.RequestToBeEmployee };

            var result = typeof(UserService)
                            .GetMethod("UserTypeCheckAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            result.Invoke(userService, new object[] { dummy.UserType, dummy });

            Assert.AreEqual(UserTypes.RequestToBeEmployee, dummy.UserType);
        }

        [Test]
        [TestCase(1, "newfirstone", "newlastone")]
        [TestCase(2, "newfirsttwo", "newlasttwo")]
        [TestCase(3, "newfirstthree", "newlastthree")]
        [TestCase(4, "newfirstfour", "newlastfour")]
        public async Task UpdateUser_ShouldSucceed_WhenUsersChangeFirstAndLastName(int id, string newFirstname, string newLastname)
        {
            await CreateDummyUsers();

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

        [Test]
        public async Task UpdateUser_ShouldSucceed_WhenEmployeeUpdatesCustomerAccounts()
        {
            await CreateDummyUsers();

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

        [Test]
        public async Task Admin_Can_Grant_Employee_Account_Request()
        {
            await CreateDummyUsers();

            LoggedInAs = userAdmin;

            userRequestToBecomeEmployee.UserType = UserTypes.Employee;
            userRequestToBecomeEmployee2.UserType = UserTypes.Employee;

            await userService.UpdateUserAsync(userRequestToBecomeEmployee.Id, userRequestToBecomeEmployee);
            await userService.UpdateUserAsync(userRequestToBecomeEmployee2.Id, userRequestToBecomeEmployee2);

            Assert.AreEqual(UserTypes.Employee, userRequestToBecomeEmployee.UserType);
            Assert.AreEqual(UserTypes.Employee, userRequestToBecomeEmployee2.UserType);
        }

        [Test]
        [TestCase(1, "<'newpassone'>", "<'newpassone'>")]
        [TestCase(2, "newpasstwo", "newpasstwo")]
        [TestCase(3, "newpassthree", "newpassthree")]
        [TestCase(4, "newpassfour", "newpassfour")]
        public async Task ChangePassword_ShouldSucceed_WhenUserEntersCorrectOld_And_CanConfirmNewPassword(int id, string newPass, string newPassConfirm)
        {
            await CreateDummyUsers();

            LoggedInAs = await unitOfWork.UserRepository.ReadAsync(id);

            Assert.IsFalse(UnHashPassword(newPass, LoggedInAs.Password));

            await userService.ChangePasswordAsync("aaaaaa", id, newPass, newPassConfirm);

            Assert.IsTrue(UnHashPassword(newPass, LoggedInAs.Password));
        }

        [Test]
        [TestCase(1, "<'newpassone'>", "<'newpassone'>")]
        [TestCase(2, "aaaaaa", "aaaaaa")]
        [TestCase(3, "aaaaaabb", "aaaaaab")]
        public async Task ChangePassword_ShouldFailWhen_OldPasswordWrong_NewPasswordConfirmationFails_UnchangedNewAndOld(int id, string newPass, string newPassConfirmed)
        {
            await CreateDummyUsers();

            LoggedInAs = await unitOfWork.UserRepository.ReadAsync(id);

            if (id == 1)
            {
                var ex = Assert.ThrowsAsync<PasswordChangeException>(async ()
                    => await userService.ChangePasswordAsync("aaaaaab", id, newPass, newPassConfirmed));

                Assert.AreEqual(ex.Message, "The old passwords don't match!");
            }
            else if (id == 2)
            {
                var ex = Assert.ThrowsAsync<PasswordChangeException>(async ()
                    => await userService.ChangePasswordAsync("aaaaaa", id, newPass, newPassConfirmed));

                Assert.AreEqual(ex.Message, "The new and old password must be different!");
            }
            else if (id == 3)
            {
                var ex = Assert.ThrowsAsync<PasswordChangeException>(async ()
                    => await userService.ChangePasswordAsync("aaaaaa", id, newPass, newPassConfirmed));

                Assert.AreEqual(ex.Message, "The new passwords don't match!");
            }
        }


        [Test]
        [TestCase(2, "newpasstwo", "aaaaaa")]
        [TestCase(3, "newpassthree", "aaaaaa")]
        public async Task ChangePassword_ShouldFail_WhenNotDoneByAccountOwner(int id, string oldPass, string newPass)
        {
            await CreateDummyUsers();

            LoggedInAs = await unitOfWork.UserRepository.ReadAsync(id - 1);

            var ex = Assert.ThrowsAsync<AccessException>(async ()
                => await userService.ChangePasswordAsync(oldPass, id, newPass, newPass));

            Assert.AreEqual(ex.Message, "Only the owner of the account can change their password!");
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public async Task GetUser_ShouldReturn_CorrectUser(int id)
        {
            await CreateDummyUsers();

            LoggedInAs = await unitOfWork.UserRepository.ReadAsync(id);

            User user = await userService.GetUserAsync(id);

            Assert.AreEqual(id, user.Id);
        }

        [Test]
        public async Task GetUser_ShouldFail_IfCustomerTriesToGetAnotherUserAccount()
        {
            await CreateDummyUsers();

            LoggedInAs = userCustomer1;

            var exc = Assert.ThrowsAsync<AccessException>(async ()
                => await userService.GetUserAsync(6));

            Assert.AreEqual(exc.Message, "Restricted access!");
        }

        [Test]
        public async Task GetUser_ShouldFail_IfNoUsersWithTheIdExists()
        {
            await CreateDummyUsers();

            LoggedInAs = userAdmin;

            var ex = Assert.ThrowsAsync<IdNotFoundException>(async ()
                => await userService.GetUserAsync(600));

            Assert.AreEqual("User with ID 600 not found!", ex.Message);
        }

        [Test]
        public async Task Only_Admin_Can_View_Admin_Account()
        {
            await CreateDummyUsers();

            LoggedInAs = userAdmin;

            Assert.DoesNotThrowAsync(async () => await userService.GetUserAsync(1));

            LoggedInAs = userEmployee1;

            var ex = Assert.ThrowsAsync<AccessException>(async ()
                => await userService.GetUserAsync(1));

            Assert.AreEqual(ex.Message, "Restricted access!");

            LoggedInAs = userRequestToBecomeEmployee;

            ex = Assert.ThrowsAsync<AccessException>(async ()
                => await userService.GetUserAsync(1));

            Assert.AreEqual(ex.Message, "Restricted access!");

            LoggedInAs = userCustomer1;

            ex = Assert.ThrowsAsync<AccessException>(async ()
                => await userService.GetUserAsync(1));

            Assert.AreEqual(ex.Message, "Restricted access!");
        }

        [Test]
        public async Task GetAllUsers_ShouldReturn_AllUsersWhenAdmin()
        {
            await CreateDummyUsers();

            LoggedInAs = userAdmin;

            var all = await userService.GetAllUsersAsync();

            Assert.AreEqual(6, all.Count);
        }

        [Test]
        public async Task GetAllUsers_ShouldReturn_AllCustomers_And_TheirOwnAccount_WhenEmployee()
        {
            await CreateDummyUsers();

            LoggedInAs = userEmployee1;

            var all = await userService.GetAllUsersAsync();

            Assert.AreEqual(3, all.Count);
            Assert.AreEqual(2, all[0].Id);
            Assert.AreEqual(UserTypes.Customer, all[1].UserType);
            Assert.AreEqual(UserTypes.Customer, all[2].UserType);
        }

        [Test]
        public async Task GetAllUsers_ShouldFail_WhenCustomer()
        {
            await CreateDummyUsers();

            LoggedInAs = userCustomer1;

            var ex = Assert.ThrowsAsync<AccessException>(async ()
                => await userService.GetAllUsersAsync());

            Assert.AreEqual(ex.Message, "Restricted access!");
        }

        [Test]
        public async Task GetAllEmployeeRequests_ShouldReturn_AllEmployeeRequestsIfAdmin()
        {
            await CreateDummyUsers();

            LoggedInAs = userAdmin;

            var all = await userService.GetAllEmployeeRequestsAsync();
            Assert.AreEqual(2, all.Count);
        }

        [Test]
        public async Task GetAllEmployeeRequests_ShouldFail_IfNotAdmin()
        {
            await CreateDummyUsers();

            LoggedInAs = userEmployee1;

            var ex = Assert.ThrowsAsync<AccessException>(async ()
                => await userService.GetAllEmployeeRequestsAsync());

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