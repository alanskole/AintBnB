using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.Core.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static AintBnB.BusinessLogic.Helpers.PasswordHashing;

[assembly: InternalsVisibleToAttribute("AintBnB.WebApi")]
namespace AintBnB.BusinessLogic.Helpers
{
    internal static class Authentication
    {
        /// <summary>Checks if the logged in user is admin.</summary>
        /// <returns>True if admin is logged in, false otherwise</returns>
        public static bool AdminChecker(UserTypes userType)
        {
            if (userType == UserTypes.Admin)
                return true;

            return false;
        }

        /// <summary>Checks if the logged in user is admin or the user that matches the one from the parameter.</summary>
        /// <param name="idOwner">The ID of the user to check.</param>
        /// <returns>True if the logged in user has the same ID as the one from the parameter or is admin, false otherwise</returns>
        public static bool CorrectUserOrAdmin(int idOwner, int loggedInAsId, UserTypes userTypeOfLoggedInUser)
        {
            if (AdminChecker(userTypeOfLoggedInUser) || idOwner == loggedInAsId)
                return true;

            return false;
        }

        /// <summary>Checks if the logged in user is admin, has the same ID as the parameter or the user that matches the user object from the parameter.</summary>
        /// <param name="idOwner">The ID of the user to check.</param>
        /// <param name="user">The object of the user to check.</param>
        /// <returns>True if the logged in user has the same ID as the one from the parameter, is the same as the user object from the parameter or is admin, false otherwise</returns>
        public static bool CorrectUserOrOwnerOrAdmin(int idOwner, int bookerId, int loggedInAsId, UserTypes userTypeLoggedInAs)
        {
            if (idOwner != loggedInAsId && bookerId != loggedInAsId)
            {
                if (!CorrectUserOrAdmin(idOwner, loggedInAsId, userTypeLoggedInAs))
                    return false;
            }
            return true;
        }

        public static bool CheckIfUserIsAllowedToPerformAction(User userThatOwnsTheObject, int idOfLoggedInUser, UserTypes userTypeOfLoggedInUser)
        {
            if (userThatOwnsTheObject.UserType != UserTypes.Customer)
                return false;

            if (userThatOwnsTheObject.Id != idOfLoggedInUser)
            {
                if (!AdminChecker(userTypeOfLoggedInUser))
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

        /// <summary>Tries to login a user.</summary>
        /// <param name="userName">Username of the user that tries to login.</param>
        /// <param name="password">The password of the user that tries to login.</param>
        /// <param name="allUsers">A list of all the in the database.</param>
        /// <exception cref="AlreadyLoggedInException">If the user is already logged in</exception>
        public static Tuple<int, UserTypes> TryToLogin(string userName, string password, List<User> allUsers)
        {
            foreach (var user in allUsers)
            {
                if (string.Equals(user.UserName, userName))
                {
                    if (VerifyPasswordHash(password, user.Password))
                    {
                        return new Tuple<int, UserTypes>(user.Id, user.UserType);
                    }
                }
            }
            throw new LoginException("Username and/or password not correct!");
        }
    }
}
