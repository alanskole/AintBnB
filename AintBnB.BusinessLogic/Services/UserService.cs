using AintBnB.Core.Models;
using AintBnB.BusinessLogic.Repository;
using System;
using System.Collections.Generic;
using AintBnB.BusinessLogic.DependencyProviderFactory;
using static AintBnB.BusinessLogic.Services.AuthenticationService;

namespace AintBnB.BusinessLogic.Services
{
    public class UserService : IUserService
    {
        private IRepository<User> _iUserRepository;

        public IRepository<User> IUserRepository
        {
            get { return _iUserRepository; }
            set
            {
                if (value == null)
                    throw new ArgumentException("IUserRepository cannot be null");
                _iUserRepository = value;
            }
        }

        public UserService()
        {
            _iUserRepository = ProvideDependencyFactory.userRepository;
        }

        public UserService(IRepository<User> userRepo)
        {
            _iUserRepository = userRepo;
        }

        public User CreateUser(string username, string password, string firstName, string lastName)
        {
            //IsUserNameFree(username);
            string hashed = HashPassword(password);
            User human = new User(username, hashed, firstName, lastName);
            _iUserRepository.Create(human);
            return human;
        }

        public User GetUser(int id)
        {
            return _iUserRepository.Read(id);
        }

        public List<User> GetAllUsers()
        {
            return _iUserRepository.GetAll();
        }

        public void UpdateUser(int id, User updatedUser)
        {
            _iUserRepository.Update(id, updatedUser);
        }
    }
}
