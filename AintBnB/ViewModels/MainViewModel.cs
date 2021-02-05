using System;
using System.Text;
using AintBnB.Core.Models;
using AintBnB.Helpers;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace AintBnB.ViewModels
{
    public class MainViewModel : Observable
    {
        private User _user = new User();

        public User User
        {
            get { return _user; }
            set
            {
                _user = value;
                NotifyPropertyChanged("User");
            }
        }

        public async Task CreateTheUser()
        {

            Uri uri = new Uri("https://localhost:44342/api/user");
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler))
            {
                string userJson = JsonConvert.SerializeObject(User);
                HttpResponseMessage response = await client.PostAsync(
                    uri, new StringContent(userJson, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
            }
            // return URI of the created resource.
            //return response.Headers.Location;
        }
    }
}
