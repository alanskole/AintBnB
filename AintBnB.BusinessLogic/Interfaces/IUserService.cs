using AintBnB.Core.Models;
using System.Collections.Generic;

namespace AintBnB.BusinessLogic.Interfaces
{
    public interface IUserService
    {
        void ValidateUser(string userName, string firstName, string lastName);
        void ChangePassword(string old, int userId, string new1, string new2);
        User CreateUser(string userName, string password, string firstName, string lastName, UserTypes userType);
        List<User> GetAllUsers();
        User GetUser(int id);
        void UpdateUser(int id, User updatedUser);
        List<User> GetAllUsersWithTypeCustomer();
        List<User> GetAllEmployeeRequests();
        List<User> GetAllUsersForLogin();
    }
}
