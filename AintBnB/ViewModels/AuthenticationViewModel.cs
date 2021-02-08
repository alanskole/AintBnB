using AintBnB.Core.Models;
using AintBnB.Helpers;
using AintBnB.Services;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AintBnB.ViewModels
{
    public class AuthenticationViewModel : Observable
    {
        private string _userName;
        private string _password;
        private HttpClientProvider _clientProvider = new HttpClientProvider();
        private string _uri;
        private string _uniquePartOfUri;

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                NotifyPropertyChanged("UserName");
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                NotifyPropertyChanged("Password");
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

            _uniquePartOfUri = "login/" + UserName.Trim() + " " + Password.Trim();
            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            ResponseChecker(response);
        }

        private static void ResponseChecker(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;
            else
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }

        private void CheckForEmptyFields()
        {
            if (UserName == null || Password == null || UserName.Trim().Length == 0 || Password.Trim().Length == 0)
                throw new ArgumentException("None of the fields can be empty");
        }

        public async Task<int> IdOfLoggedInUser()
        {
            User user = new User();

            _uniquePartOfUri = "loggedin";
            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            if (response.IsSuccessStatusCode)
            {
                string jsonUser = await response.Content.ReadAsStringAsync();
                user = JsonConvert.DeserializeObject<User>(jsonUser);
                return user.Id;
            }
            throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }

        public async Task LogoutFromApp()
        {
            _uniquePartOfUri = "logout";
            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            ResponseChecker(response);
        }
    }
}
