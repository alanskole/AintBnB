using AintBnB.CommonMethodsAndProperties;
using AintBnB.Core.Models;
using AintBnB.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AintBnB.CommonMethodsAndProperties.CommonViewModelMethods;

namespace AintBnB.ViewModels
{
    public class UserViewModel : Observable
    {
        private User _user = new User();
        private HttpClientProvider _clientProvider = new HttpClientProvider();
        private string _uri;
        private string _uniquePartOfUri;
        private string _passwordConfirm;
        private List<User> _allEmployeeRequests;
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

        public List<User> AllEmployeeRequests
        {
            get { return _allEmployeeRequests; }
            set
            {
                _allEmployeeRequests = value;
                NotifyPropertyChanged("AllEmployeeRequests");
            }
        }

        public UserViewModel()
        {
            _clientProvider.ControllerPartOfUri = "api/user/";
            _uri = _clientProvider.LocalHostAddress + _clientProvider.LocalHostPort + _clientProvider.ControllerPartOfUri;
        }

        public async Task MakeEmployeeAsync()
        {
            User.UserType = UserTypes.Employee;
            await UpdateAUserAsync();
        }

        public void RequestToBecomeEmployee()
        {
            User.UserType = UserTypes.RequestToBeEmployee;
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

            _user = await GetAsync<User>(_uri + _uniquePartOfUri, _clientProvider);

            NotifyPropertyChanged("User");
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await GetAllAsync<User>(_uri, _clientProvider);
        }

        public async Task<List<User>> GetAllCustomersAsync()
        {
            _uniquePartOfUri = "allcustomers";

            return await GetAllAsync<User>(_uri + _uniquePartOfUri, _clientProvider);
        }

        public async Task<List<User>> GetAllEmployeeRequestsAsync()
        {
            _uniquePartOfUri = "requests";

            AllEmployeeRequests = await GetAllAsync<User>(_uri + _uniquePartOfUri, _clientProvider);

            return AllEmployeeRequests;
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
