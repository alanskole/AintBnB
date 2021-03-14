using AintBnB.Helpers;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using AintBnB.CommonMethodsAndProperties;
using static AintBnB.CommonMethodsAndProperties.CommonViewModelMethods;

namespace AintBnB.ViewModels
{
    public class WorldViewModel : Observable
    {
        private HttpClientProvider _clientProvider = new HttpClientProvider();
        private string _uri;
        private string _uniquePartOfUri;
        private string _country;
        private string _city;

        public string Country
        {
            get { return _country; }
            set
            {
                _country = value;
                NotifyPropertyChanged("Country");
            }
        }

        public string City
        {
            get { return _city; }
            set
            {
                _city = value;
                NotifyPropertyChanged("City");


            }
        }

        public WorldViewModel()
        {
            _clientProvider.ControllerPartOfUri = "api/world/";
            _uri = _clientProvider.LocalHostAddress + _clientProvider.LocalHostPort + _clientProvider.ControllerPartOfUri;
        }

        public async Task<List<string>> GetAllCountriesInTheWorld()
        {
            _uniquePartOfUri = "countries";

            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            ResponseChecker(response);
            string jsonUsers = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<string>>(jsonUsers);
        }

        public async Task<List<string>> GetAllCitiesOfACountry()
        {
            _uniquePartOfUri = "cities/" + Country;

            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            ResponseChecker(response);
            string jsonUsers = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<string>>(jsonUsers);
        }
    }
}
