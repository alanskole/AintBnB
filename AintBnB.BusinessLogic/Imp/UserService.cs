using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Core.Models;
using AintBnB.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AintBnB.BusinessLogic.Helpers.Authentication;
using static AintBnB.BusinessLogic.Helpers.Regexp;
namespace AintBnB.BusinessLogic.Imp
{
    public class UserService : IUserService
    {
        private IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>Creates a new user.</summary>
        /// <param name="userName">Username of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="firstName">The first name of the user.</param>
        /// <param name="lastName">The last name of the user.</param>
        /// <param name="userType">The usertype (role) of the user.</param>
        /// <returns>The new user object that was created</returns>
        public async Task<User> CreateUserAsync(string userName, string password, string firstName, string lastName, UserTypes userType)
        {
            userName = userName.Trim();
            firstName = firstName.Trim();
            lastName = lastName.Trim();

            await IsUserNameFreeAsync(userName);
            ValidateUser(userName, firstName, lastName);
            ValidatePassword(password);

            var user = new User();
            user.Password = HashPassword(password);
            user.UserName = userName;
            user.FirstName = firstName;
            user.LastName = lastName;

            await _unitOfWork.UserRepository.CreateAsync(user);

            await UserTypeCheckAsync(userType, user);

            await _unitOfWork.CommitAsync();
            return user;
        }

        /// <summary>Checks which usertype the new user should be.</summary>
        /// <param name="userType">Usertype that was requested at creation time.</param>
        /// <param name="user">The object of the user that will be created.</param>
        private async Task UserTypeCheckAsync(UserTypes userType, User user)
        {
            var all = await _unitOfWork.UserRepository.GetAllAsync();

            if (all.Count == 0)
                user.UserType = UserTypes.Admin;
            else
                user.UserType = UserTypes.Customer;
        }

        /// <summary>Validates the properties of the user to be created.</summary>
        /// <param name="userName">Username of the user.</param>
        /// <param name="firstName">The first name  of the user.</param>
        /// <param name="lastName">The last name  of the user.</param>
        /// <exception cref="ParameterException">
        /// UserName can't empty
        /// or
        /// FirstName can't contain any other characters than letters and one space or dash between names
        /// or
        /// LastName can't contain any other characters than letters and one space or dash between names
        /// </exception>
        public void ValidateUser(string userName, string firstName, string lastName)
        {
            if (userName == null || userName.Length == 0)
                throw new ParameterException("UserName", "empty");
            if (!onlyLettersOneSpaceOrDash.IsMatch(firstName))
                throw new ParameterException("FirstName", "containing any other than letters and one space or dash between names");
            if (!onlyLettersOneSpaceOrDash.IsMatch(lastName))
                throw new ParameterException("LastName", "containing any other than letters and one space or dash between names");
        }

        /// <summary>Checks if the desired username is available or already used by another user.</summary>
        /// <param name="userName">The username to check.</param>
        /// <exception cref="ParameterException">Username already taken by another user</exception>
        private async Task IsUserNameFreeAsync(string userName)
        {
            foreach (var user in await _unitOfWork.UserRepository.GetAllAsync())
            {
                if (string.Equals(user.UserName, userName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ParameterException("Username already taken!");
                }
            }
        }

        /// <summary>Fetches a user.</summary>
        /// <param name="id">The ID of the user to get.</param>
        /// <returns>The user object</returns>
        /// <exception cref="NotFoundException">No users found with the provided ID</exception>
        public async Task<User> GetUserAsync(int id)
        {
            var user = await _unitOfWork.UserRepository.ReadAsync(id);

            if (user == null)
                throw new NotFoundException("User", id);

            return user;
        }

        /// <summary>Gets all users.</summary>
        /// <returns>A list of all the users</returns>
        public async Task<List<User>> GetAllUsersAsync()
        {
            var all = await _unitOfWork.UserRepository.GetAllAsync();
            IsListEmpty(all);
            return all;
        }

        /// <summary>Gets all users with usertype customer.</summary>
        /// <returns>A list of all the users with usertype customer</returns>
        /// <exception cref="NotFoundException">If no users found</exception>
        public async Task<List<User>> GetAllUsersWithTypeCustomerAsync()
        {
            var all = new List<User>();

            foreach (var user in await _unitOfWork.UserRepository.GetAllAsync())
            {
                if (user.UserType == UserTypes.Customer)
                    all.Add(user);
            }
            IsListEmpty(all);

            return all;
        }

        private static void IsListEmpty(List<User> all)
        {
            if (all.Count == 0)
                throw new NotFoundException("users");
        }

        /// <summary>Updates a user.</summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="updatedUser">The updated user object.</param>
        /// <param name="userTypeOfLoggedInUser">The usertype of the user sending the request.</param>
        public async Task UpdateUserAsync(int id, User updatedUser, UserTypes userTypeOfLoggedInUser)
        {
            var old = await GetUserAsync(id);

            ValidateUser(old.UserName, updatedUser.FirstName, updatedUser.LastName);

            if (updatedUser.UserType != old.UserType)
                CanUserTypeBeUpdated(userTypeOfLoggedInUser);

            updatedUser.UserName = old.UserName;

            await _unitOfWork.UserRepository.UpdateAsync(id, updatedUser);
            await _unitOfWork.CommitAsync();
        }

        /// <summary>Determines whether the usertype can be updated.</summary>
        /// <param name="userTypeOfLoggedInUser">The usertype of the user sending the request.</param>
        /// <exception cref="AccessException">Only admin can change the usertype of a user</exception>
        private static void CanUserTypeBeUpdated(UserTypes userTypeOfLoggedInUser)
        {
            if (userTypeOfLoggedInUser != UserTypes.Admin)
                throw new AccessException("Only admin can change usertype!");
        }

        /// <summary>Changes the password of a user.</summary>
        /// <param name="old">The original password.</param>
        /// <param name="userId">The ID of the user to change the password of.</param>
        /// <param name="new1">The new password.</param>
        /// <param name="new2">The new password confirmed.</param>
        /// <exception cref="PasswordChangeException">new or old</exception>
        public async Task ChangePasswordAsync(string old, int userId, string new1, string new2)
        {
            var user = await GetUserAsync(userId);

            if (old == new1)
                throw new PasswordChangeException();

            string hashedOriginalPassword = user.Password;

            if (new1 != new2)
                throw new PasswordChangeException("new");

            ValidatePassword(new1);

            if (VerifyPasswordHash(old, hashedOriginalPassword))
            {
                user.Password = HashPassword(new1);
                await _unitOfWork.CommitAsync();
            }
            else
                throw new PasswordChangeException("old");
        }
    }
}
