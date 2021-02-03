using AintBnB.Core.Models;
using System;
using AintBnB.BusinessLogic.DependencyProviderFactory;
using BCrypt.Net;

namespace AintBnB.BusinessLogic.Services
{
    static public class AuthenticationService
    {
        public static User LoggedInAs;

        public static void AnyoneLoggedIn()
        {
            if (LoggedInAs == null)
                throw new ArgumentException("Not logged in!");
        }

        public static void AdminChecker()
        {
            AnyoneLoggedIn();

            if (LoggedInAs.UserType != UserTypes.Admin)
                throw new ArgumentException("Administrator only!");
        }

        public static void CorrectUser(int id)
        {
            AnyoneLoggedIn();

            if (id != LoggedInAs.Id)
                AdminChecker();
        }

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool ValidatePassword(string password, string correctHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, correctHash);
        }

        public static void TryToLogin(string userName, string password)
        {
            foreach (User user in ProvideDependencyFactory.userRepository.GetAll())
            {
                if (string.Equals(user.UserName, userName))
                {
                    if (ValidatePassword(password, user.Password))
                    {
                        LoggedInAs = user;
                        return;
                    }
                    throw new ArgumentException("Password not correct!");
                }
            }
            throw new ArgumentException("Username and/or password not correct!");
        }

        public static void IsUserNameFree(string userName)
        {
            foreach (User user in ProvideDependencyFactory.userRepository.GetAll())
            {
                if (string.Equals(user.UserName, userName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Username already taken!");
                }
            }   
        }
    }
}