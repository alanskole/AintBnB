using AintBnB.Core.Models;
using AintBnB.Helpers;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AintBnB.ViewModels
{
    public class LoginViewModel : Observable
    {
        private string _userName;
        private string _password;

        public string UserName
        {
            get { return _userName; }
            set
            {
                if (value == null)
                    throw new ArgumentException("Username cannot be null");
                _userName = value;
                NotifyPropertyChanged("UserName");
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (value == null)
                    throw new ArgumentException("Password cannot be null");
                _password = value;
                NotifyPropertyChanged("Password");
            }
        }

        public async Task<bool> Login()
        {
            string userPass = UserName.Trim() + " " + Password.Trim();
            Uri uri = new Uri("https://localhost:44342/api/authentication/login/" + userPass);
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler))
            {
                HttpResponseMessage response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                    return true;
                return false;
            }
        }

        public async Task<int> IdOfLoggedInUser()
        {
            User user = new User();
            Uri uri = new Uri("https://localhost:44342/api/authentication/loggedin/");
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler))
            {
                HttpResponseMessage response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    string jsonUser = await response.Content.ReadAsStringAsync();
                    user = JsonConvert.DeserializeObject<User>(jsonUser);
                }
                return user.Id;
            }
        }

        public async Task<bool> LogoutFromApp()
        {
            Uri uri = new Uri("https://localhost:44342/api/authentication/logout/");
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler))
            {
                HttpResponseMessage response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                    return true;
                return false;
            }
        }
    }
}
