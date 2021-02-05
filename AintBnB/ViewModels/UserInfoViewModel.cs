using AintBnB.Core.Models;
using AintBnB.Helpers;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Net.Http.Headers;

namespace AintBnB.ViewModels
{
    public class UserInfoViewModel : Observable
    {
        private int _userId;
        private User _user;

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

        public async Task GetAUser()
        {
            Uri uri = new Uri("https://localhost:44342/api/user/" + UserId);
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler))
            {
                HttpResponseMessage response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    _user = new User();
                    string jsonUser = await response.Content.ReadAsStringAsync();
                    _user = JsonConvert.DeserializeObject<User>(jsonUser);
                    NotifyPropertyChanged("User");
                }
            }
        }

        public async Task<List<User>> GetAllUsers()
        {
            List<User> _all = new List<User>(); ;
            Uri uri = new Uri("https://localhost:44342/api/user");
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler))
            {
                HttpResponseMessage response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    string jsonUsers = await response.Content.ReadAsStringAsync();
                    _all = JsonConvert.DeserializeObject<List<User>>(jsonUsers);
                }
                return _all;
            }
        }
    }
}
