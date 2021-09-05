using AintBnB.App.CommonMethodsAndProperties;
using AintBnB.App.Helpers;
using AintBnB.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AintBnB.App.CommonMethodsAndProperties.ApiCalls;
using static AintBnB.App.Helpers.UwpCookieHelper;

namespace AintBnB.App.ViewModels
{
    public class UserViewModel : Observable
    {
        private User _user = new User();
        private string _uri = "user/";
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

        public async Task CreateTheUserAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                if (User.Password != PasswordConfirm)
                    throw new Exception("The passwords don't match!");

                await GetCsrfToken(_clientProvider);

                await PostAsync(_uri, User, _clientProvider);
            }
        }

        public async Task GetAUserAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var uniquePartOfUri = User.Id.ToString();

                await AddAuthCookieAsync(_clientProvider.clientHandler);

                User = await GetAsync<User>(_uri + uniquePartOfUri, _clientProvider);
            }
        }

        public async Task GetAllUsersAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                await AddAuthCookieAsync(_clientProvider.clientHandler);

                AllUsers = await GetAllAsync<User>(_uri, _clientProvider);
            }
        }

        public async Task GetAllCustomersAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var uniquePartOfUri = "allcustomers";

                await AddAuthCookieAsync(_clientProvider.clientHandler);

                AllUsers = await GetAllAsync<User>(_uri + uniquePartOfUri, _clientProvider);
            }
        }

        public async Task DeleteAUserAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var uniquePartOfUri = User.Id.ToString();

                await AddAuthCookieAsync(_clientProvider.clientHandler);

                await GetCsrfToken(_clientProvider);

                await DeleteAsync(_uri + uniquePartOfUri, _clientProvider);
            }
        }

        public async Task UpdateAUserAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var uniquePartOfUri = User.Id.ToString();

                await AddAuthCookieAsync(_clientProvider.clientHandler);

                await GetCsrfToken(_clientProvider);

                await PutAsync(_uri + uniquePartOfUri, User, _clientProvider);
            }
        }
    }
}
