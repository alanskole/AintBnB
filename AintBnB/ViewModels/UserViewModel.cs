using AintBnB.Core.Models;
using AintBnB.Helpers;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using AintBnB.Services;
using System.Text;

namespace AintBnB.ViewModels
{
    public class UserViewModel : Observable
    {
        private int _userId;
        private User _user = new User();
        private HttpClientProvider _clientProvider = new HttpClientProvider();
        private string _uri;
        private string _uniquePartOfUri;
        public int UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                NotifyPropertyChanged("UserId");
            }
        }

        public User User
        {
            get { return _user; }
        }

        public UserViewModel()
        {
            _clientProvider.ControllerPartOfUri = "api/user/";
            _uri = _clientProvider.LocalHostAddress + _clientProvider.LocalHostPort + _clientProvider.ControllerPartOfUri;
        }

        public async Task CreateTheUser()
        {
            string userJson = JsonConvert.SerializeObject(User);
            HttpResponseMessage response = await _clientProvider.client.PostAsync(
                _uri, new StringContent(userJson, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }

        public async Task GetAUser()
        {
            _uniquePartOfUri = UserId.ToString();


            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            if (response.IsSuccessStatusCode)
            {
                string jsonUser = await response.Content.ReadAsStringAsync();
                _user = JsonConvert.DeserializeObject<User>(jsonUser);
                NotifyPropertyChanged("User");
            }
            else
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }

        public async Task<List<User>> GetAllUsers()
        {
            List<User> _all = new List<User>();


            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri));
            if (response.IsSuccessStatusCode)
            {
                string jsonUsers = await response.Content.ReadAsStringAsync();
                _all = JsonConvert.DeserializeObject<List<User>>(jsonUsers);
                return _all;
            }
            throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }
    }
}
