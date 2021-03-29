using AintBnB.Core.Models;
using System;
using AintBnB.BusinessLogic.CustomExceptions;
using System.Collections.Generic;
using System.Net;
using static AintBnB.BusinessLogic.Helpers.PasswordHashing;

namespace AintBnB.BusinessLogic.Helpers
{
    public static class Authentication
    {
        public static User LoggedInAs;
        public static void AnyoneLoggedIn()
        {
            if (LoggedInAs == null)
                throw new LoginException("Not logged in!");
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

        public static bool CorrectUserOrAdminOrEmployee(User user)
        {
            if (AdminChecker())
                return true;

            if (user.Id == LoggedInAs.Id)
                return true;

            if (EmployeeChecker())
            {
                if (user.UserType == UserTypes.Customer)
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

        public static bool CorrectUserOrOwnerOrAdminOrEmployee(int idOwner, User user)
        {
            AnyoneLoggedIn();

            if (idOwner != LoggedInAs.Id)
            {
                if (!CorrectUserOrAdminOrEmployee(user))
                    return false;
            }
            return true;
        }

        public static bool CheckIfUserIsAllowedToPerformAction(User user)
        {
            AnyoneLoggedIn();

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
            return HashThePassword(password, null, false);
        }

        public static bool UnHashPassword(string password, string correctHash)
        {
            return VerifyThePassword(password, correctHash);
        }

        public static void ValidatePassword(string password)
        {
            if (password.Contains(" "))
                throw new PasswordException("can't contain spaces");
            if (password.Length < 6)
                throw new PasswordException("must contain minimum 6 characters");
            if (password.Length > 50)
                throw new PasswordException("must contain maximum 50 characters");
        }

        public static void Logout()
        {
            LoggedInAs = null;
        }

        public static void TryToLogin(string userName, string password, List<User> allUsers)
        {
            try
            {
                AnyoneLoggedIn();
            }
            catch (Exception)
            {
                LoginUser(userName, password, allUsers);
                return;
            }
            throw new AlreadyLoggedInException();
        }

        private static void LoginUser(string userName, string password, List<User> allUsers)
        {
            foreach (User user in allUsers)
            {
                if (string.Equals(user.UserName, userName))
                {
                    if (user.UserType == UserTypes.RequestToBeEmployee)
                        throw new LoginException("The request to have an employee account must be approved by admin before it can be used!");

                    if (UnHashPassword(password, user.Password))
                    {
                        LoggedInAs = user;
                        return;
                    }
                }
            }
            throw new LoginException("Username and/or password not correct!");
        }
    }
}
