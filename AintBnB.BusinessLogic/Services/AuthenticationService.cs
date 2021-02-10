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

        public static void CorrectUserOrOwner(int idOwner, int userId)
        {
            try
            {
                AnyoneLoggedIn();
            }
            catch (Exception)
            {
                throw;
            }

            if (idOwner != LoggedInAs.Id)
            {
                try
                {
                    CorrectUser(userId);
                }
                catch (Exception)
                {
                    throw new AccessException(idOwner, userId);
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