using AintBnB.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AintBnB.BusinessLogic.Interfaces
{
    public interface IUserService
    {
        void ValidateUser(string userName, string firstName, string lastName);
        Task ChangePasswordAsync(string old, int userId, string new1, string new2);
        Task<User> CreateUserAsync(string userName, string password, string firstName, string lastName, UserTypes userType);
        Task<List<User>> GetAllUsersAsync();
        Task<User> GetUserAsync(int id);
        Task UpdateUserAsync(int id, User updatedUser);
        Task<List<User>> GetAllUsersWithTypeCustomerAsync();
        Task<List<User>> GetAllEmployeeRequestsAsync();
        Task<List<User>> GetAllUsersForLoginAsync();
    }
}
