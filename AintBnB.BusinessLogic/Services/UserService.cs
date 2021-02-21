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

        public User CreateUser(string userName, string password, string firstName, string lastName, UserTypes userType)
        {

            IsUserNameFree(userName);
            ValidateUser(userName, firstName, lastName);
            ValidatePassword(password);

            User user = new User();
            user.Password = HashPassword(password.Trim());
            user.UserName = userName.Trim();
            user.FirstName = firstName.Trim();
            user.LastName = lastName.Trim();

            _iUserRepository.Create(user);

            if (user.Id == 1)
                user.UserType = UserTypes.Admin;
            else if (userType == UserTypes.RequestToBeEmployee)
                user.UserType = UserTypes.RequestToBeEmployee;

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
            if (CorrectUserOrAdminOrEmployee(id))
            {
                User user = _iUserRepository.Read(id);

                if (user == null)
                    throw new IdNotFoundException("User", id);

                OnlyAdminCanViewAdmin(user);

                return user;
            }
            throw new AccessException();
        }

        private static void OnlyAdminCanViewAdmin(User user)
        {
            if (user.UserType == UserTypes.Admin)
            {
                if (!AdminChecker())
                    throw new AccessException("Only admin can view admin");
            }
        }

        public List<User> GetAllUsers()
        {
            if (AdminChecker())
                return AdminCanGetAllUsers();

            List<User> allCustomersPlusLoggedinEmployee = GetAllUsersWithTypeCustomer();

            allCustomersPlusLoggedinEmployee.Insert(0, LoggedInAs);

            return allCustomersPlusLoggedinEmployee;
        }

        private List<User> AdminCanGetAllUsers()
        {
            List<User> all = _iUserRepository.GetAll();
            IsListEmpty(all);
            return all;
        }

        public List<User> GetAllUsersWithTypeCustomer()
        {


            if (!HasElevatedRights())
                throw new AccessException();

            List<User> all = new List<User>();

            foreach (var user in _iUserRepository.GetAll())
            {
                if (user.UserType == UserTypes.Customer)
                    all.Add(user);
            }
            IsListEmpty(all);

            return all;
        }

        public List<User> GetAllEmployeeRequests()
        {

            if (!AdminChecker())
                throw new AccessException("Admin only!");

            List<User> all = new List<User>();

            foreach (var user in _iUserRepository.GetAll())
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

        public void UpdateUser(int id, User updatedUser)
        {
            User old;

            if (CorrectUserOrAdminOrEmployee(id))
            {
                old = GetUser(id);
                ValidateUser(old.UserName, updatedUser.FirstName, updatedUser.LastName);

                if (updatedUser.UserType != old.UserType)
                    CanUserTypeBeUpdated();

                updatedUser.UserName = old.UserName;

                _iUserRepository.Update(id, updatedUser);
            }
            else
                throw new AccessException();
        }

        private static void CanUserTypeBeUpdated()
        {
            if (!AdminChecker())
                throw new AccessException("Only admin can change usertype!");
        }

        public void ChangePassword(string old, int userId, string new1, string new2)
        {
            if (CorrectUserOrAdminOrEmployee(userId))
            {
                if (old == new1)
                    throw new PasswordChangeException();

                User user = GetUser(userId);

                string hashedOriginalPassword = user.Password;

                if (new1 != new2)
                    throw new PasswordChangeException("new");

                ValidatePassword(new1);

                if (UnHashPassword(old, hashedOriginalPassword))
                    user.Password = HashPassword(new1);
                else
                    throw new PasswordChangeException("old");
            }
            else
                throw new AccessException();
        }
    }
}
