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
                ValidateUser(userName, password, firstName, lastName);  
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
            return user;
        }

        public void ValidateUser(string userName, string password, string firstName, string lastName)
        {
            try
            {
                IsUserNameFree(userName);
                ValidatePassword(password);
            }
            catch (Exception)
            {
                throw;
            }

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

        private static void ValidatePassword(string password)
        {
            if (password.Trim().Contains(" "))
                throw new LoginExcrption("Cannot contain space");
            if (password.Trim().Length < 6)
                throw new LoginExcrption("Minimum 6 characters");
            if (password.Trim().Length > 50)
                throw new LoginExcrption("Maximum 50 characters");
        }

        public User GetUser(int id)
        {
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
            try
            {
                GetUser(id);
                ValidateUser(updatedUser.UserName, updatedUser.Password, updatedUser.FirstName, updatedUser.LastName);
            }
            catch (Exception)
            {
                throw;
            }

            _iUserRepository.Update(id, updatedUser);
        }
    }
}
