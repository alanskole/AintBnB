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

        private async Task UserTypeCheckAsync(UserTypes userType, User user)
        {
            var all = await _unitOfWork.UserRepository.GetAllAsync();

            if (all.Count == 0)
                user.UserType = UserTypes.Admin;
            else if (userType == UserTypes.RequestToBeEmployee)
                user.UserType = UserTypes.RequestToBeEmployee;
            else
                user.UserType = UserTypes.Customer;
        }

        public void ValidateUser(string userName, string firstName, string lastName)
        {
            if (userName == null || userName.Length == 0)
                throw new ParameterException("UserName", "empty");
            if (!onlyLettersOneSpaceOrDash.IsMatch(firstName))
                throw new ParameterException("FirstName", "containing any other than letters and one space or dash between names");
            if (!onlyLettersOneSpaceOrDash.IsMatch(lastName))
                throw new ParameterException("LastName", "containing any other than letters and one space or dash between names");
        }

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

        public async Task<User> GetUserAsync(int id)
        {
            var user = await _unitOfWork.UserRepository.ReadAsync(id);

            if (user == null)
                throw new IdNotFoundException("User", id);

            if (CorrectUserOrAdminOrEmployee(user))
            {
                return user;
            }
            throw new AccessException();
        }


        public async Task<List<User>> GetAllUsersForLoginAsync()
        {
            if (LoggedInAs != null)
                throw new AlreadyLoggedInException();

            try
            {
                return await AdminCanGetAllUsersAsync();
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            if (AdminChecker())
                return await AdminCanGetAllUsersAsync();

            var allCustomersPlusLoggedinEmployee = await GetAllUsersWithTypeCustomerAsync();

            allCustomersPlusLoggedinEmployee.Insert(0, LoggedInAs);

            return allCustomersPlusLoggedinEmployee;
        }

        private async Task<List<User>> AdminCanGetAllUsersAsync()
        {
            var all = await _unitOfWork.UserRepository.GetAllAsync();
            IsListEmpty(all);
            return all;
        }

        public async Task<List<User>> GetAllUsersWithTypeCustomerAsync()
        {
            if (!HasElevatedRights())
                throw new AccessException();

            var all = new List<User>();

            foreach (var user in await _unitOfWork.UserRepository.GetAllAsync())
            {
                if (user.UserType == UserTypes.Customer)
                    all.Add(user);
            }
            IsListEmpty(all);

            return all;
        }

        public async Task<List<User>> GetAllEmployeeRequestsAsync()
        {

            if (!AdminChecker())
                throw new AccessException("Admin only!");

            var all = new List<User>();

            foreach (var user in await _unitOfWork.UserRepository.GetAllAsync())
            {
                if (user.UserType == UserTypes.RequestToBeEmployee)
                    all.Add(user);
            }

            if (all.Count == 0)
                throw new NoneFoundInDatabaseTableException("requests to become employee");

            return all;
        }

        private static void IsListEmpty(List<User> all)
        {
            if (all.Count == 0)
                throw new NoneFoundInDatabaseTableException("users");
        }

        public async Task UpdateUserAsync(int id, User updatedUser)
        {
            var old = await GetUserAsync(id);

            if (CorrectUserOrAdminOrEmployee(old))
            {
                ValidateUser(old.UserName, updatedUser.FirstName, updatedUser.LastName);

                if (updatedUser.UserType != old.UserType)
                    CanUserTypeBeUpdated();

                updatedUser.UserName = old.UserName;

                await _unitOfWork.UserRepository.UpdateAsync(id, updatedUser);
                await _unitOfWork.CommitAsync();
            }
            else
                throw new AccessException();
        }

        private static void CanUserTypeBeUpdated()
        {
            if (!AdminChecker())
                throw new AccessException("Only admin can change usertype!");
        }

        public async Task ChangePasswordAsync(string old, int userId, string new1, string new2)
        {
            var user = await _unitOfWork.UserRepository.ReadAsync(userId);

            if (LoggedInAs.Id == user.Id)
            {
                if (old == new1)
                    throw new PasswordChangeException();

                string hashedOriginalPassword = user.Password;

                if (new1 != new2)
                    throw new PasswordChangeException("new");

                ValidatePassword(new1);

                if (UnHashPassword(old, hashedOriginalPassword))
                {
                    user.Password = HashPassword(new1);
                    await _unitOfWork.CommitAsync();
                }
                else
                    throw new PasswordChangeException("old");
            }
            else
                throw new AccessException("Only the owner of the account can change their password!");
        }
    }
}
