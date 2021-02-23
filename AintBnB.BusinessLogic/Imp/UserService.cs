using AintBnB.Core.Models;
using System;
using System.Collections.Generic;
using AintBnB.BusinessLogic.CustomExceptions;
using static AintBnB.BusinessLogic.Helpers.Authentication;
using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Repository.Interfaces;

namespace AintBnB.BusinessLogic.Imp
{
    public class UserService : IUserService
    {
        private IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

            _unitOfWork.UserRepository.Create(user);

            if (user.Id == 1)
                user.UserType = UserTypes.Admin;
            else if (userType == UserTypes.RequestToBeEmployee)
                user.UserType = UserTypes.RequestToBeEmployee;
            _unitOfWork.Commit();
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
            foreach (User user in _unitOfWork.UserRepository.GetAll())
            {
                if (string.Equals(user.UserName, userName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new LoginExcrption("Username already taken!");
                }
            }
        }

        public User GetUser(int id)
        {
            User user = _unitOfWork.UserRepository.Read(id);

            if (user == null)
                throw new IdNotFoundException("User", id);

            if (CorrectUserOrAdminOrEmployee(user))
            {
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
            try
            {
                AnyoneLoggedIn();
            }
            catch (Exception)
            {
                return AdminCanGetAllUsers();
            }

            if (AdminChecker())
                return AdminCanGetAllUsers();

            List<User> allCustomersPlusLoggedinEmployee = GetAllUsersWithTypeCustomer();

            allCustomersPlusLoggedinEmployee.Insert(0, LoggedInAs);

            return allCustomersPlusLoggedinEmployee;
        }

        private List<User> AdminCanGetAllUsers()
        {
            List<User> all = _unitOfWork.UserRepository.GetAll();
            IsListEmpty(all);
            return all;
        }

        public List<User> GetAllUsersWithTypeCustomer()
        {


            if (!HasElevatedRights())
                throw new AccessException();

            List<User> all = new List<User>();

            foreach (var user in _unitOfWork.UserRepository.GetAll())
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

            foreach (var user in _unitOfWork.UserRepository.GetAll())
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
            User old = GetUser(id);

            if (CorrectUserOrAdminOrEmployee(old))
            {
                ValidateUser(old.UserName, updatedUser.FirstName, updatedUser.LastName);

                if (updatedUser.UserType != old.UserType)
                    CanUserTypeBeUpdated();

                updatedUser.UserName = old.UserName;

                _unitOfWork.UserRepository.Update(id, updatedUser);
                _unitOfWork.Commit();
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
            User user = GetUser(userId);

            if (CorrectUserOrAdminOrEmployee(user))
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
                    _unitOfWork.Commit();
                }
                else
                    throw new PasswordChangeException("old");
            }
            else
                throw new AccessException();
        }
    }
}
