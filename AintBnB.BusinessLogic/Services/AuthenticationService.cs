using AintBnB.Core.Models;
using System;
using AintBnB.BusinessLogic.DependencyProviderFactory;
using BCrypt.Net;
using AintBnB.BusinessLogic.CustomExceptions;

namespace AintBnB.BusinessLogic.Services
{
    static public class AuthenticationService
    {
        public static User LoggedInAs;
        public static void AnyoneLoggedIn()
        {
            if (LoggedInAs == null)
                throw new LoginExcrption("Not logged in!");
        }

        public static void AdminChecker()
        {
            try
            {
                AnyoneLoggedIn();
            }
            catch (Exception)
            {
                throw;
            }

            if (LoggedInAs.UserType != UserTypes.Admin)
                throw new AccessException();
        }

        public static void CorrectUser(int id)
        {
            try
            {
                AnyoneLoggedIn();
            }
            catch (Exception)
            {
                throw;
            }

            if (id != LoggedInAs.Id)
            {
                try
                {
                    AdminChecker();
                }
                catch (Exception)
                {
                    throw new AccessException(id);
                }
            }
        }

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool UnHashPassword(string password, string correctHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, correctHash);
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
            foreach (User user in ProvideDependencyFactory.userRepository.GetAll())
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