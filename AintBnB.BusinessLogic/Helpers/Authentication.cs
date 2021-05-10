using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.Core.Models;
using System;
using System.Collections.Generic;
using static AintBnB.BusinessLogic.Helpers.PasswordHashing;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleToAttribute("AintBnB.WebApi")]
namespace AintBnB.BusinessLogic.Helpers
{
    internal static class Authentication
    {
        /// <summary>A static variable that contains the user that is logged in.</summary>
        public static User LoggedInAs;


        /// <summary>Checks if anyone is logged in.</summary>
        /// <exception cref="LoginException">Not logged in!</exception>
        public static void AnyoneLoggedIn()
        {
            if (LoggedInAs == null)
                throw new LoginException("Not logged in!");
        }


        /// <summary>Checks if the logged in user is admin.</summary>
        /// <returns>True if admin is logged in, false otherwise</returns>
        public static bool AdminChecker()
        {
            AnyoneLoggedIn();

            if (LoggedInAs.UserType == UserTypes.Admin)
                return true;

            return false;
        }

        /// <summary>Checks if the logged in user is an employee.</summary>
        /// <returns>True if an employee is logged in, false otherwise</returns>
        public static bool EmployeeChecker()
        {
            AnyoneLoggedIn();

            if (LoggedInAs.UserType == UserTypes.Employee)
                return true;

            return false;
        }

        /// <summary>Checks if the logged in user matching the one in the parameter or is an admin or employee.</summary>
        /// <returns>True if the user in the parameter is the one logged in, or admin or employee is logged in, false otherwise</returns>
        /// <param name="user">The user that must be the one that's logged in.</param>
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

        /// <summary>Checks if admin or employee is logged in.</summary>
        /// <returns>True if admin or employee is logged in, false otherwise.</returns>
        public static bool HasElevatedRights()
        {
            if (AdminChecker() || EmployeeChecker())
                return true;

            return false;
        }

        /// <summary>Checks if the logged in user is admin or the user that matches the one from the parameter.</summary>
        /// <param name="id">The ID of the user to check.</param>
        /// <returns>True if the logged in user has the same ID as the one from the parameter or is admin, false otherwise</returns>
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

        /// <summary>Checks if the logged in user is admin, employee, has the same ID as the parameter or the user that matches the user object from the parameter.</summary>
        /// <param name="idOwner">The ID of the user to check.</param>
        /// <param name="user">The object of the user to check.</param>
        /// <returns>True if the logged in user has the same ID as the one from the parameter, is the same as the user object from the parameter, is employee or admin, false otherwise</returns>
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

        /// <summary>Hashes a cleartext password.</summary>
        /// <param name="password">The password in cleartext.</param>
        /// <returns>The hashed password</returns>
        public static string HashPassword(string password)
        {
            return HashThePassword(password, null, false);
        }

        /// <summary>Checks if the unhashed password is the same as the hashed passwords.</summary>
        /// <param name="password">The unhashed password.</param>
        /// <param name="correctHash">The hashed password.</param>
        /// <returns>True if password is correct, false otherwise</returns>
        public static bool VerifyPasswordHash(string password, string correctHash)
        {
            return VerifyThePassword(password, correctHash);
        }

        /// <summary>Validates the password.</summary>
        /// <param name="password">The password in cleartext.</param>
        /// <exception cref="PasswordException">Password can't contain spaces
        /// or
        /// Password must contain minimum 6 characters
        /// or
        /// Password must contain maximum 50 characters</exception>
        public static void ValidatePassword(string password)
        {
            if (password.Contains(" "))
                throw new PasswordException("can't contain spaces");
            if (password.Length < 6)
                throw new PasswordException("must contain minimum 6 characters");
            if (password.Length > 50)
                throw new PasswordException("must contain maximum 50 characters");
        }

        /// <summary>Logs out the user.</summary>
        public static void Logout()
        {
            LoggedInAs = null;
        }

        /// <summary>Tries to login a user.</summary>
        /// <param name="userName">Username of the user that tries to login.</param>
        /// <param name="password">The password of the user that tries to login.</param>
        /// <param name="allUsers">A list of all the in the database.</param>
        /// <exception cref="AlreadyLoggedInException">If the user is already logged in</exception>
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

        /// <summary>Logins the user.</summary>
        /// <param name="userName">Username of the user that tries to login.</param>
        /// <param name="password">The password of the user that tries to login.</param>
        /// <param name="allUsers">A list of all the in the database.</param>
        /// <exception cref="LoginException">If the account is of usertype employeerequest that hasn't been apporved yet
        /// or
        /// Username and/or password are incorrect</exception>
        private static void LoginUser(string userName, string password, List<User> allUsers)
        {
            foreach (User user in allUsers)
            {
                if (string.Equals(user.UserName, userName))
                {
                    if (user.UserType == UserTypes.RequestToBeEmployee)
                        throw new LoginException("The request to have an employee account must be approved by admin before it can be used!");

                    if (VerifyPasswordHash(password, user.Password))
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
