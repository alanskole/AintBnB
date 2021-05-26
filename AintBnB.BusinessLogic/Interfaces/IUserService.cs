using AintBnB.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AintBnB.BusinessLogic.Interfaces
{
    public interface IUserService
    {
        /// <summary>Validates the properties of the user to be created.</summary>
        /// <param name="userName">Username of the user.</param>
        /// <param name="firstName">The first name  of the user.</param>
        /// <param name="lastName">The last name  of the user.</param>
        void ValidateUser(string userName, string firstName, string lastName);

        /// <summary>Changes the password of a user.</summary>
        /// <param name="old">The original password.</param>
        /// <param name="userId">The ID of the user to change the password of.</param>
        /// <param name="new1">The new password.</param>
        /// <param name="new2">The new password confirmed.</param>
        Task ChangePasswordAsync(string old, int userId, string new1, string new2);

        /// <summary>Creates a new user.</summary>
        /// <param name="userName">Username of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="firstName">The first name of the user.</param>
        /// <param name="lastName">The last name of the user.</param>
        /// <param name="userType">The usertype (role) of the user.</param>
        /// <returns>The new user object that was created</returns>
        Task<User> CreateUserAsync(string userName, string password, string firstName, string lastName, UserTypes userType);

        /// <summary>Gets all users.</summary>
        /// <returns>A list of all the users</returns>
        Task<List<User>> GetAllUsersAsync();

        /// <summary>Fetches a user.</summary>
        /// <param name="id">The ID of the user to get.</param>
        /// <returns>The user object</returns>
        Task<User> GetUserAsync(int id);

        /// <summary>Updates a user.</summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="updatedUser">The updated user object.</param>
        Task UpdateUserAsync(int id, User updatedUser);

        /// <summary>Gets all users with usertype customer.</summary>
        /// <returns>A list of all the users with usertype customer</returns>
        Task<List<User>> GetAllUsersWithTypeCustomerAsync();

        /// <summary>Gets all users. Used when logging in.</summary>
        /// <returns>A list with all the users</returns> 
        Task<List<User>> GetAllUsersForLoginAsync();
    }
}
