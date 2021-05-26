using AintBnB.App.CommonMethodsAndProperties;
using AintBnB.App.Helpers;
using AintBnB.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AintBnB.App.CommonMethodsAndProperties.ApiCalls;

namespace AintBnB.App.ViewModels
{
    public class UserViewModel : Observable
    {
        private User _user = new User();
        private HttpClientProvider _clientProvider = new HttpClientProvider();
        private string _uri;
        private string _uniquePartOfUri;
        private string _passwordConfirm;
        private List<User> _allUsers;

        public User User
        {
            get { return _user; }
            set
            {
                _user = value;
                NotifyPropertyChanged("User");
            }
        }

        public string PasswordConfirm
        {
            get { return _passwordConfirm; }
            set
            {
                _passwordConfirm = value;
                NotifyPropertyChanged("PasswordConfirm");
            }
        }

        public List<User> AllUsers
        {
            get { return _allUsers; }
            set
            {
                _allUsers = value;
                NotifyPropertyChanged("AllUsers");
            }
        }

        public UserViewModel()
        {
            _clientProvider.ControllerPartOfUri = "api/user/";
            _uri = _clientProvider.LocalHostAddress + _clientProvider.LocalHostPort + _clientProvider.ControllerPartOfUri;
        }

        public async Task CreateTheUserAsync()
        {
            if (User.Password != PasswordConfirm)
                throw new Exception("The passwords don't match!");

            await PostAsync(_uri, User, _clientProvider);
        }

        public async Task GetAUserAsync()
        {
            _uniquePartOfUri = User.Id.ToString();

            User = await GetAsync<User>(_uri + _uniquePartOfUri, _clientProvider);
        }

        public async Task GetAllUsersAsync()
        {
            AllUsers = await GetAllAsync<User>(_uri, _clientProvider);
        }

        public async Task GetAllCustomersAsync()
        {
            _uniquePartOfUri = "allcustomers";

            AllUsers = await GetAllAsync<User>(_uri + _uniquePartOfUri, _clientProvider);
        }

        public async Task DeleteAUserAsync()
        {
            _uniquePartOfUri = User.Id.ToString();

            await DeleteAsync(_uri + _uniquePartOfUri, _clientProvider);
        }

        public async Task UpdateAUserAsync()
        {
            _uniquePartOfUri = User.Id.ToString();

            await PutAsync(_uri + _uniquePartOfUri, User, _clientProvider);
        }
    }
}
