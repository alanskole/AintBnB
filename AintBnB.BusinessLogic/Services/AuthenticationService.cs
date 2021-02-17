using AintBnB.Core.Models;
using System;
using AintBnB.BusinessLogic.DependencyProviderFactory;
using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Repository;

namespace AintBnB.BusinessLogic.Services
{
    static public class AuthenticationService
    {
        private static IRepository<User> _userRepository = ProvideDependencyFactory.userRepository;

        public static User LoggedInAs;
        public static void AnyoneLoggedIn()
        {
            if (LoggedInAs == null)
                throw new LoginExcrption("Not logged in!");
        }

        public static bool AdminChecker()
        {
            AnyoneLoggedIn();

            if (LoggedInAs.UserType == UserTypes.Admin)
                return true;

            return false;
        }

        public static bool EmployeeChecker()
        {
            AnyoneLoggedIn();

            if (LoggedInAs.UserType == UserTypes.Employee)
                return true;

            return false;
        }

        public static bool EmployeeCanOnlyHandleCustomerAccounts(int id)
        {
            if (EmployeeChecker())
            {
                if (_userRepository.Read(id).UserType == UserTypes.Customer || id == LoggedInAs.Id)
                    return true;
            }
            return false;
        }

        public static bool CorrectUserOrAdminOrEmployee(int id)
        {
            if (AdminChecker())
                return true;

            if (id == LoggedInAs.Id)
                return true;

            User user = _userRepository.Read(id);

            if (EmployeeChecker())
            {
                if (user.UserType != UserTypes.Admin && user.UserType != UserTypes.Employee)
                    return true;
            }

            return false;
        }

        public static bool HasElevatedRights()
        {
            if (AdminChecker() || EmployeeChecker())
                return true;

            return false;
        }

        public static bool CorrectUserOrAdmin(int id)
        {
            if (EmployeeChecker())
                return false;

            if (AdminChecker())
                return true;

            if (id == LoggedInAs.Id)
                return true;

            return false;
        }

        public static bool CorrectUserOrOwnerOrAdminOrEmployee(int idOwner, int userId)
        {
            AnyoneLoggedIn();

            if (idOwner != LoggedInAs.Id)
            {
                if (!CorrectUserOrAdminOrEmployee(userId))
                    return false;
            }
            return true;
        }

        public static bool CheckIfUserIsAllowedToPerformAction(int id)
        {
            AnyoneLoggedIn();

            User user = _userRepository.Read(id);

            if (user.UserType != UserTypes.Customer)
                return false;

            if (user.Id != LoggedInAs.Id)
            {
                if (!HasElevatedRights())
                    return false;
            }
            return true;
        }

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool UnHashPassword(string password, string correctHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, correctHash);
        }

        public static void ValidatePassword(string password)
        {
            if (password.Trim().Contains(" "))
                throw new LoginExcrption("Cannot contain space");
            if (password.Trim().Length < 6)
                throw new LoginExcrption("Minimum 6 characters");
            if (password.Trim().Length > 50)
                throw new LoginExcrption("Maximum 50 characters");
        }

        public static void Logout()
        {
            LoggedInAs = null;
        }

        public static void TryToLogin(string userName, string password)
        {
            try
            {
                AnyoneLoggedIn();
            }
            catch (Exception)
            {
                LoginUser(userName, password);
                return;
            }
            throw new AlreadyLoggedInException();
        }

        private static void LoginUser(string userName, string password)
        {
            foreach (User user in _userRepository.GetAll())
            {
                if (string.Equals(user.UserName, userName))
                {
                    if (UnHashPassword(password, user.Password))
                    {
                        LoggedInAs = user;
                        return;
                    }
                }
            }
            throw new LoginExcrption("Username and/or password not correct!");
        }
    }
}