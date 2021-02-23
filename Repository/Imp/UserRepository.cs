using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AintBnB.Repository.Imp
{
    public class UserRepository : IRepository<User>
    {
        private DatabaseContext _databaseContext;

        public UserRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        public void Create(User user)
        {
            _databaseContext.User.Add(user);
        }

        public void Delete(int id)
        {
            _databaseContext.User.Remove(Read(id));
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
        }
    }
}
