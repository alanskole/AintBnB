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
            userName = userName.Trim();
            password = password.Trim();
            firstName = firstName.Trim();
            lastName = lastName.Trim();

            IsUserNameFree(userName);
            ValidateUser(userName, firstName, lastName);
            ValidatePassword(password);

            User user = new User();
            user.Password = HashPassword(password);
            user.UserName = userName;
            user.FirstName = firstName;
            user.LastName = lastName;

            _unitOfWork.UserRepository.Create(user);

            UserTypeCheck(userType, user);

            _unitOfWork.Commit();
            return user;
        }

        private void UserTypeCheck(UserTypes userType, User user)
        {
            if (_unitOfWork.UserRepository.GetAll().Count == 0)
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

        private void IsUserNameFree(string userName)
        {
            foreach (User user in _unitOfWork.UserRepository.GetAll())
            {
                if (string.Equals(user.UserName, userName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ParameterException("Username already taken!");
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
                return user;
            }
            throw new AccessException();
        }


        public List<User> GetAllUsersForLogin()
        {
            if (LoggedInAs != null)
                throw new AlreadyLoggedInException();

            try
            {
                return AdminCanGetAllUsers();
            }
            catch (Exception)
            {
                throw;
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
            User user = _unitOfWork.UserRepository.Read(userId);

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
                    _unitOfWork.Commit();
                }
                else
                    throw new PasswordChangeException("old");
            }
            else
                throw new AccessException("Only the owner of the account can change their password!");
        }
    }
}
