using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.BusinessLogic.DependencyProviderFactory;
using System.Collections.Generic;
using System.Linq;

namespace AintBnB.BusinessLogic.Repository
{
    public class UserRepository : IRepository<User>
    {
        public readonly DatabaseContext _databaseContext = ProvideDependencyFactory.databaseContext;

        public void Create(User user)
        {
            _databaseContext.User.Add(user);
            _databaseContext.SaveChanges();
        }

        public void Delete(int id)
        {
            _databaseContext.User.Remove(Read(id));
            _databaseContext.SaveChanges();
        }

        public List<User> GetAll()
        {
            return _databaseContext.User.ToList();
        }

        public User Read(int id)
        {
            return _databaseContext.User.Find(id);
        }

        public void Update(int id, User updatedUser)
        {
            var user = _databaseContext.User.Find(id);
            user.UserName = updatedUser.UserName;
            user.Password = updatedUser.Password;
            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.UserType = updatedUser.UserType;
            _databaseContext.SaveChanges();
        }
    }
}
