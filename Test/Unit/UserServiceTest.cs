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
        public async Task CreateUserAsync_ShouldReturn_NewlyCreatedUser()
        {
            var first = await userService.CreateUserAsync("adm1", "aaaaaa", "adminfirstname", "adminlastname", UserTypes.Admin);
            var second = await userService.CreateUserAsync("cust2", "aaaaaa", "anothercustomerfirstname", "anothercustomerlastname", UserTypes.Customer);

            Assert.AreEqual(1, first.Id);
            Assert.AreEqual("adm1", first.UserName);
            Assert.AreEqual(UserTypes.Admin, first.UserType);
            Assert.AreEqual("cust2", second.UserName);
            Assert.AreEqual(UserTypes.Customer, second.UserType);
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
            var dummy = new User { UserType = UserTypes.Customer };

            var result = typeof(UserService)
                .GetMethod("UserTypeCheckAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            await (Task)result.Invoke(userService, new object[] { dummy.UserType, dummy });

            Assert.AreEqual(UserTypes.Admin, dummy.UserType);
        }

        [TestMethod]
        public async Task UserTypeCheckAsync_ShouldSetUserTypeToCustomer_IfNotRequestToBeOrFirstCreatedUser()
        {
            await CreateDummyUsersAsync();

            var dummy = new User { UserType = UserTypes.Admin };

            var result = typeof(UserService)
                .GetMethod("UserTypeCheckAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            await (Task)result.Invoke(userService, new object[] { dummy.UserType, dummy });

            Assert.AreEqual(UserTypes.Customer, dummy.UserType);
        }

        [DataRow(1, "newfirstone", "newlastone")]
        [DataRow(2, "newfirsttwo", "newlasttwo")]
        [DataRow(3, "newfirstthree", "newlastthree")]
        [DataRow(4, "newfirstfour", "newlastfour")]
        [DataTestMethod]
        public async Task UpdateUserAsync_ShouldSucceed_WhenUsersChangeFirstAndLastName(int id, string newFirstname, string newLastname)
        {
            await CreateDummyUsersAsync();

            var user = await userService.GetUserAsync(id);

            Assert.AreNotEqual(user.FirstName, newFirstname);
            Assert.AreNotEqual(user.LastName, newLastname);

            user.FirstName = newFirstname;
            user.LastName = newLastname;

            await userService.UpdateUserAsync(id, user, user.UserType);

            user = await userService.GetUserAsync(id);

            Assert.AreEqual(user.FirstName, newFirstname);
            Assert.AreEqual(user.LastName, newLastname);
        }

        [DataRow(1, "<'newpassone'>", "<'newpassone'>")]
        [DataRow(2, "newpasstwo", "newpasstwo")]
        [DataRow(3, "newpassthree", "newpassthree")]
        [DataRow(4, "newpassfour", "newpassfour")]
        [DataTestMethod]
        public async Task ChangePasswordAsync_ShouldSucceed_WhenUserEntersCorrectOld_And_CanConfirmNewPassword(int id, string newPass, string newPassConfirm)
        {
            await CreateDummyUsersAsync();

            var user = await unitOfWork.UserRepository.ReadAsync(id);

            Assert.IsFalse(VerifyPasswordHash(newPass, user.Password));

            await userService.ChangePasswordAsync("aaaaaa", id, newPass, newPassConfirm);

            Assert.IsTrue(VerifyPasswordHash(newPass, user.Password));
        }

        [DataRow(1, "<'newpassone'>", "<'newpassone'>")]
        [DataRow(2, "aaaaaa", "aaaaaa")]
        [DataRow(3, "aaaaaabb", "aaaaaab")]
        [DataTestMethod]
        public async Task ChangePasswordAsync_ShouldFailWhen_OldPasswordWrong_NewPasswordConfirmationFails_UnchangedNewAndOld(int id, string newPass, string newPassConfirmed)
        {
            await CreateDummyUsersAsync();

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

        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataTestMethod]
        public async Task GetUserAsync_ShouldReturn_CorrectUser(int id)
        {
            await CreateDummyUsersAsync();

            var user = await userService.GetUserAsync(id);

            Assert.AreEqual(id, user.Id);
        }

        [TestMethod]
        public async Task GetUserAsync_ShouldFail_IfNoUsersWithTheIdExists()
        {
            await CreateDummyUsersAsync();

            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(async ()
                => await userService.GetUserAsync(600));

            Assert.AreEqual("User with ID 600 not found!", ex.Message);
        }

        [TestMethod]
        public async Task GetAllUsersAsync_ShouldReturn_AllUsers()
        {
            await CreateDummyUsersAsync();

            var all = await userService.GetAllUsersAsync();

            Assert.AreEqual(4, all.Count);
        }

        [TestMethod]
        public async Task GetAllUsersAsync_ShouldFail_WhenNoUsersExist()
        {
            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(async ()
                => await userService.GetAllUsersAsync());

            Assert.AreEqual(ex.Message, "No users found!");
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