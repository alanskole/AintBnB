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
        private UserTypes _userTypeOfLoggedInUser;
        private bool _alreadyFetchedLoggedInUser = false;
        private bool _isLoggedIn = false;
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

        public UserTypes UserTypeOfLoggedInUser
        {
            get { return _userTypeOfLoggedInUser; }
            set
            {
                _userTypeOfLoggedInUser = value;
                NotifyPropertyChanged("UserTypeOfLoggedInUser");
            }
        }

        public bool AlreadyFetchedLoggedInUser
        {
            get { return _alreadyFetchedLoggedInUser; }
            set
            {
                _alreadyFetchedLoggedInUser = value;
                NotifyPropertyChanged("AlreadyFetchedLoggedInUser");
            }
        }

        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set
            {
                _isLoggedIn = value;
                NotifyPropertyChanged("NotLoggedIn");
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
            if (!AlreadyFetchedLoggedInUser)
                await LoggedInUserIdAndUserType();

            return IsLoggedIn;
        }

        public async Task LoggedInUserIdAndUserType()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var uniquePartOfUri = "currentUserIdAndRole";

                await AddAuthCookieAsync(_clientProvider.clientHandler);

                try
                {
                    var user = await GetAsync<User>(_uri + uniquePartOfUri, _clientProvider);

                    IdOfLoggedInUser = user.Id;

                    UserTypeOfLoggedInUser = user.UserType;

                    AlreadyFetchedLoggedInUser = true;

                    IsLoggedIn = true;
                }
                catch
                {
                }
            }
        }

        public async Task IsAdminAsync()
        {
            if (!AlreadyFetchedLoggedInUser)
                await LoggedInUserIdAndUserType();

            if (UserTypeOfLoggedInUser != UserTypes.Admin)
                throw new Exception("Not admin!");
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
