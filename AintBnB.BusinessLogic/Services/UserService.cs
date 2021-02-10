using AintBnB.Core.Models;
using AintBnB.BusinessLogic.Repository;
using System;
using System.Collections.Generic;
using AintBnB.BusinessLogic.DependencyProviderFactory;
using AintBnB.BusinessLogic.CustomExceptions;
using static AintBnB.BusinessLogic.Services.AuthenticationService;

namespace AintBnB.BusinessLogic.Services
{
    public class UserService : IUserService
    {
        private IRepository<User> _iUserRepository;

        public IRepository<User> IUserRepository
        {
            get { return _iUserRepository; }
            set
            {
                if (value == null)
                    throw new ArgumentException("IUserRepository cannot be null");
                _iUserRepository = value;
            }
        }

        public UserService()
        {
            _iUserRepository = ProvideDependencyFactory.userRepository;
        }

        public UserService(IRepository<User> userRepo)
        {
            _iUserRepository = userRepo;
        }

        public User CreateUser(string userName, string password, string firstName, string lastName)
        {

            try
            {
                IsUserNameFree(userName);
                ValidateUser(userName, firstName, lastName);
                ValidatePassword(password);
            }
            catch (Exception)
            {
                throw;
            }

            User user = new User();
            user.Password = HashPassword(password.Trim());
            user.UserName = userName.Trim();
            user.FirstName = firstName.Trim();
            user.LastName = lastName.Trim();

            _iUserRepository.Create(user);

            if (user.Id == 1)
                user.UserType = UserTypes.Admin;

            return user;
        }

        public void ValidateUser(string userName, string firstName, string lastName)
        {
            if (userName == null || userName.Trim().Length == 0)
                throw new ParameterException("UserName", "empty");
            if (firstName == null || firstName.Trim().Length == 0)
                throw new ParameterException("FirstName", "empty");
            if (lastName == null || lastName.Trim().Length == 0)
                throw new ParameterException("LastName", "empty");
        }

        private void IsUserNameFree(string userName)
        {
            foreach (User user in _iUserRepository.GetAll())
            {
                if (string.Equals(user.UserName, userName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new LoginExcrption("Username already taken!");
                }
            }
        }

        public User GetUser(int id)
        {
            try
            {
                CorrectUser(id);
            }
            catch (Exception)
            {

                throw;
            }

            User user = _iUserRepository.Read(id);

            if (user == null)
                throw new IdNotFoundException("User", id);

            return user;
        }

        public List<User> GetAllUsers()
        {
            try
            {
                AdminChecker();
            }
            catch (Exception)
            {

                throw;
            }

            List<User> all = _iUserRepository.GetAll();

            if (all.Count == 0)
                throw new NoneFoundInDatabaseTableException("users");

            return all;
        }

        public void UpdateUser(int id, User updatedUser)
        {
            User old;

            try
            {
                CorrectUser(id);
                old = GetUser(id);
                ValidateUser(old.UserName, updatedUser.FirstName, updatedUser.LastName);
            }
            catch (Exception)
            {
                throw;
            }

            updatedUser.UserName = old.UserName;

            _iUserRepository.Update(id, updatedUser);
        }

        public void ChangePassword(string old, int userId, string new1, string new2)
        {
            try
            {
                CorrectUser(userId);
            }
            catch (Exception)
            {

                throw;
            }

            if (old == new1)
                throw new PasswordChangeException();

            string hashedOriginalPassword = GetUser(userId).Password;

            if (new1 != new2)
                throw new PasswordChangeException("new");

            try
            {
                ValidatePassword(new1);
            }
            catch (Exception)
            {
                throw;
            }

            if (UnHashPassword(old, hashedOriginalPassword))
                GetUser(userId).Password = HashPassword(new1);
            else
                throw new PasswordChangeException("old");
        }
    }
}
