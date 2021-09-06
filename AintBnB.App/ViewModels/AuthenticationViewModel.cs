using AintBnB.App.CommonMethodsAndProperties;
using AintBnB.App.Helpers;
using AintBnB.Core.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static AintBnB.App.CommonMethodsAndProperties.ApiCalls;
using static AintBnB.App.Helpers.UwpCookieHelper;

namespace AintBnB.App.ViewModels
{
    public class AuthenticationViewModel : Observable
    {
        private int _idOfLoggedInUser;
        private User _user = new User();
        private string _uri = "authentication/";

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

        public async Task LoginAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                CheckForEmptyFields();

                var uniquePartOfUri = "login";

                await GetCsrfToken(_clientProvider);

                var userAndPass = new string[] { User.UserName.Trim(), User.Password.Trim() };

                await PostAsync(_uri + uniquePartOfUri, userAndPass, _clientProvider);

                await AddCookiesToLocalSettings(_clientProvider.clientHandler, new Uri(_clientProvider.client.BaseAddress + _uri + uniquePartOfUri));
            }
        }

        private async Task AddCookiesToLocalSettings(HttpClientHandler clientHandler, Uri uri)
        {
            var responseCookies = clientHandler.CookieContainer.GetCookies(uri).Cast<Cookie>();

            try
            {
                localSettings.Values[DefaultCookieName] = await EncryptCookieValueAsync(responseCookies.Single(o => o.Name == DefaultCookieName).Value);
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

        public async Task<bool> IsUserLoggedInAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var uniquePartOfUri = "isLoggedIn";

                await AddAuthCookieAsync(_clientProvider.clientHandler);

                try
                {
                    await GetAsync(_uri + uniquePartOfUri, _clientProvider);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public async Task IdOfLoggedInUserAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var uniquePartOfUri = "loggedinUserId";

                await AddAuthCookieAsync(_clientProvider.clientHandler);

                IdOfLoggedInUser = await GetAsync<int>(_uri + uniquePartOfUri, _clientProvider);
            }
        }

        public async Task IsAdminAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var uniquePartOfUri = "admin";

                await AddAuthCookieAsync(_clientProvider.clientHandler);
                try
                {
                    await GetAsync(_uri + uniquePartOfUri, _clientProvider);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task LogoutFromAppAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var uniquePartOfUri = "logout";

                await AddAuthCookieAsync(_clientProvider.clientHandler);

                await GetCsrfToken(_clientProvider);

                await PostAsync(_uri + uniquePartOfUri, null, _clientProvider);

                if (localSettings.Values.ContainsKey(DefaultCookieName))
                {
                    var cookieObj = await DecryptCookieValueAsync(localSettings.Values[DefaultCookieName].ToString());
                    if (cookieObj != null)
                    {
                        var authCookie = new Cookie(DefaultCookieName, cookieObj.ToString());
                        authCookie.Expires = DateTime.Now.Subtract(TimeSpan.FromDays(1));
                        localSettings.Values.Remove(DefaultCookieName);
                    }
                }
            }
        }
    }
}
