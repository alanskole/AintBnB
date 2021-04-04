using AintBnB.CommonMethodsAndProperties;
using AintBnB.Core.Models;
using AintBnB.Helpers;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static AintBnB.CommonMethodsAndProperties.CommonViewModelMethods;

namespace AintBnB.ViewModels
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

        public async Task Login()
        {
            CheckForEmptyFields();

            _uniquePartOfUri = "login";
            string[] userAndPass = new string[] { User.UserName.Trim(), User.Password.Trim() };
            string loginJson = JsonConvert.SerializeObject(userAndPass);
            HttpResponseMessage response = await _clientProvider.client.PostAsync(
                (_uri + _uniquePartOfUri), new StringContent(loginJson, Encoding.UTF8, "application/json"));
            ResponseChecker(response);
        }


        public async Task IsAnyoneLoggedIn()
        {
            _uniquePartOfUri = "anyoneloggedin";
            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            ResponseChecker(response);
        }

        private void CheckForEmptyFields()
        {
            if (User.UserName == null || User.Password == null || User.UserName.Trim().Length == 0 || User.Password.Trim().Length == 0)
                throw new ArgumentException("None of the fields can be empty");
        }

        public async Task<int> IdOfLoggedInUser()
        {
            _uniquePartOfUri = "loggedin";
            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            ResponseChecker(response);
            string jsonUser = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<User>(jsonUser).Id;
        }

        public async Task IsAdmin()
        {
            _uniquePartOfUri = "admin";

            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            ResponseChecker(response);
        }

        public async Task IsEmployee()
        {
            _uniquePartOfUri = "employee";

            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            ResponseChecker(response);
        }

        public async Task IsEmployeeOrAdmin()
        {
            _uniquePartOfUri = "elevatedrights";

            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            ResponseChecker(response);
        }

        public async Task LogoutFromApp()
        {
            _uniquePartOfUri = "logout";
            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            ResponseChecker(response);
        }
    }
}
