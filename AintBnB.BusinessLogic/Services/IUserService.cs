using AintBnB.Core.Models;
using System.Collections.Generic;

namespace AintBnB.BusinessLogic.Services
{
    public interface IUserService
    {
        void ValidateUser(string userName, string firstName, string lastName);
        void ValidatePassword(string password);
        void ChangePassword(string old, int userId, string new1, string new2);
        User CreateUser(string userName, string password, string firstName, string lastName);
        List<User> GetAllUsers();
        User GetUser(int id);
        void UpdateUser(int id, User updatedUser);
    }
}
