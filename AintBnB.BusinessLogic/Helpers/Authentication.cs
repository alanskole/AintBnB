using AintBnB.Core.Models;
using System;
using AintBnB.BusinessLogic.CustomExceptions;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net;

namespace AintBnB.BusinessLogic.Helpers
{
    public static class Authentication
    {
        public static Regex onlyLettersOneSpaceOrDash = new Regex(@"^([A-Za-z\u00C0-\u00D6\u00D8-\u00f6\u00f8-\u00ff]+([\s-]?[A-Za-z\u00C0-\u00D6\u00D8-\u00f6\u00f8-\u00ff]+){1,})$");
        public static Regex onlyLettersNumbersOneSpaceOrDash = new Regex(@"^([A-Za-z\u00C0-\u00D6\u00D8-\u00f6\u00f8-\u00ff0-9]([\s-]?[A-Za-z\u00C0-\u00D6\u00D8-\u00f6\u00f8-\u00ff0-9]+)*){2,}$");
        public static Regex onlyNumbersFollowedByAnOptionalLetter = new Regex(@"^[1-9]+[0-9]*[A-Za-z]?$");
        public static Regex zipCodeFormatsOfTheWorld = new Regex(@"^[A-Za-z\u00C0-\u00D6\u00D8-\u00f6\u00f8-\u00ff0-9][A-Za-z\u00C0-\u00D6\u00D8-\u00f6\u00f8-\u00ff0-9\- ]{1,11}$");
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
            return BCrypt.Net.BCrypt.HashPassword(WebUtility.HtmlEncode(password));
        }

        public static bool UnHashPassword(string password, string correctHash)
        {
            return BCrypt.Net.BCrypt.Verify(WebUtility.HtmlEncode(password), correctHash);
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
                        throw new LoginExcrption("The request to have an employee account must be approved by admin before it can be used!");

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