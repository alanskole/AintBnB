using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.Repository.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AintBnB.Repository.Imp
{
    public class UserRepository : IRepository<User>
    {
        private DatabaseContext _databaseContext;

        public UserRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        public async Task CreateAsync(User user)
        {
            await _databaseContext.User.AddAsync(user);
        }

        public async Task DeleteAsync(int id)
        {
            _databaseContext.User.Remove(await ReadAsync(id));
        }

        public async Task<List<User>> GetAllAsync()
        {
            return _databaseContext.User.ToList();
        }

        public async Task<User> ReadAsync(int id)
        {
            return await _databaseContext.User.FindAsync(id);
        }

        public async Task UpdateAsync(int id, User updatedUser)
        {
            var user = await ReadAsync(id);
            user.UserName = updatedUser.UserName;
            user.Password = updatedUser.Password;
            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.UserType = updatedUser.UserType;
        }
    }
}
