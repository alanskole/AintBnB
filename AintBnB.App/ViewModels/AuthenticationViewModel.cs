using AintBnB.App.CommonMethodsAndProperties;
using AintBnB.Core.Models;
using AintBnB.App.Helpers;
using System;
using System.Threading.Tasks;
using static AintBnB.App.CommonMethodsAndProperties.ApiCalls;

namespace AintBnB.App.ViewModels
{
    public class AuthenticationViewModel : Observable
    {
        private User _user = new User();
        private HttpClientProvider _clientProvider = new HttpClientProvider();
        private string _uri;
        private string _uniquePartOfUri;

        public User User
        {
            get { return _user; }
            set
            {
                _user = value;
                NotifyPropertyChanged("User");
            }
        }

        public AuthenticationViewModel()
        {
            _clientProvider.ControllerPartOfUri = "api/authentication/";
            _uri = _clientProvider.LocalHostAddress + _clientProvider.LocalHostPort + _clientProvider.ControllerPartOfUri;
        }

        public async Task LoginAsync()
        {
            CheckForEmptyFields();

            _uniquePartOfUri = "login";

            var userAndPass = new string[] { User.UserName.Trim(), User.Password.Trim() };

            await PostAsync(_uri + _uniquePartOfUri, userAndPass, _clientProvider);
        }


        public async Task IsAnyoneLoggedInAsync()
        {
            _uniquePartOfUri = "anyoneloggedin";
            await GetAsync(_uri + _uniquePartOfUri, _clientProvider);
        }

        private void CheckForEmptyFields()
        {
            if (User.UserName == null || User.Password == null || User.UserName.Trim().Length == 0 || User.Password.Trim().Length == 0)
                throw new ArgumentException("None of the fields can be empty");
        }

        public async Task<int> IdOfLoggedInUserAsync()
        {
            _uniquePartOfUri = "loggedin";

            var user = await GetAsync<User>(_uri + _uniquePartOfUri, _clientProvider);
            return user.Id;
        }

        public async Task IsAdminAsync()
        {
            _uniquePartOfUri = "admin";

            await GetAsync(_uri + _uniquePartOfUri, _clientProvider);
        }

        public async Task IsEmployeeOrAdminAsync()
        {
            _uniquePartOfUri = "elevatedrights";

            await GetAsync(_uri + _uniquePartOfUri, _clientProvider);
        }

        public async Task LogoutFromAppAsync()
        {
            _uniquePartOfUri = "logout";
            await GetAsync(_uri + _uniquePartOfUri, _clientProvider);
        }
    }
}
