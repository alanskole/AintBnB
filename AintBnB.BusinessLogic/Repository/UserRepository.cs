using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.BusinessLogic.DependencyProviderFactory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AintBnB.BusinessLogic.Repository
{
    public class UserRepository : IRepository<User>
    {
        public readonly DatabaseContext _databaseContext = ProvideDependencyFactory.databaseContext;

        public void Create(User humanoid)
        {
            _databaseContext.Users.Add(humanoid);
            _databaseContext.SaveChanges();
        }

        public void Delete(int id)
        {
            _databaseContext.Users.Remove(Read(id));
            _databaseContext.SaveChanges();
        }

        public List<User> GetAll()
        {
            return _databaseContext.Users.ToList();
        }

        public User Read(int id)
        {
            return _databaseContext.Users.Find(id);
        }

        public void Update(int id, User humanoid)
        {
            var user = _databaseContext.Users.Find(id);
            user.UserName = humanoid.UserName;
            user.Password = humanoid.Password;
            user.FirstName = humanoid.FirstName;
            user.LastName = humanoid.LastName;
            user.UserType = humanoid.UserType;
            _databaseContext.SaveChanges();
        }
    }
}
