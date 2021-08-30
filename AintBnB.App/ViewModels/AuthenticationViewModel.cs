using AintBnB.App.CommonMethodsAndProperties;
using AintBnB.App.Helpers;
using AintBnB.Core.Models;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static AintBnB.App.CommonMethodsAndProperties.ApiCalls;
using static AintBnB.App.Helpers.UwpCookieHelper;

namespace AintBnB.App.ViewModels
{
    public class AuthenticationViewModel : Observable
    {
        private int _idOfLoggedInUser;
        private User _user = new User();
        private HttpClientProvider _clientProvider = new HttpClientProvider();
        private string _uri;
        private string _uniquePartOfUri;

        public int IdOfLoggedInUser
        {
            get { return _idOfLoggedInUser; }
            set
            {
                _idOfLoggedInUser = value;
                NotifyPropertyChanged("IdOfLoggedInUser");
            }
        }

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

            await GetCsrfToken(_clientProvider);

            var userAndPass = new string[] { User.UserName.Trim(), User.Password.Trim() };

            await PostAsync(_uri + _uniquePartOfUri, userAndPass, _clientProvider);

            var responseCookies = _clientProvider.clientHandler.CookieContainer.GetCookies(new Uri(_uri + _uniquePartOfUri)).Cast<Cookie>();
            try
            {
                localSettings.Values["myCoockie"] = await EncryptCookieValueAsync(responseCookies.Single(o => o.Name == "myCoockie").Value);
            }
            catch
            {
            }
        }

        private void CheckForEmptyFields()
        {
            if (User.UserName == null || User.Password == null || User.UserName.Trim().Length == 0 || User.Password.Trim().Length == 0)
                throw new ArgumentException("None of the fields can be empty");
        }

        public async Task IdOfLoggedInUserAsync()
        {
            _uniquePartOfUri = "loggedin";

            await AddAuthCookieAsync(_clientProvider.clientHandler);

            IdOfLoggedInUser = await GetAsync<int>(_uri + _uniquePartOfUri, _clientProvider);
        }

        public async Task IsAdminAsync()
        {
            _uniquePartOfUri = "admin";

            await AddAuthCookieAsync(_clientProvider.clientHandler);
            try
            {
                await GetAsync(_uri + _uniquePartOfUri, _clientProvider);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task LogoutFromAppAsync()
        {
            _uniquePartOfUri = "logout";

            await AddAuthCookieAsync(_clientProvider.clientHandler);

            await GetAsync(_uri + _uniquePartOfUri, _clientProvider);

            if (localSettings.Values.ContainsKey("myCoockie"))
            {
                var cookieObj = await DecryptCookieValueAsync(localSettings.Values["myCoockie"].ToString());
                if (cookieObj != null)
                {
                    var authCookie = new Cookie("myCoockie", cookieObj.ToString());
                    authCookie.Expires = DateTime.Now.Subtract(TimeSpan.FromDays(1));
                    localSettings.Values.Remove("myCoockie");
                }
            }
        }
    }
}
