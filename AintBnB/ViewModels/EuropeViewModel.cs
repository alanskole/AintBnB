using AintBnB.Core.Models;
using AintBnB.Helpers;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using AintBnB.CommonMethodsAndProperties;

namespace AintBnB.ViewModels
{
    public class EuropeViewModel : Observable
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

        public EuropeViewModel()
        {
            _clientProvider.ControllerPartOfUri = "api/europe/";
            _uri = _clientProvider.LocalHostAddress + _clientProvider.LocalHostPort + _clientProvider.ControllerPartOfUri;

        }

        public async Task<List<string>> GetAllCountriesInEurope()
        {
            List<string> _all = new List<string>();

            _uniquePartOfUri = "countries";

            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            if (response.IsSuccessStatusCode)
            {
                string jsonUsers = await response.Content.ReadAsStringAsync();
                _all = JsonConvert.DeserializeObject<List<string>>(jsonUsers);
                return _all;
            }
            throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }

        public async Task<List<string>> GetAllCitiesOfACountry()
        {
            List<string> _all = new List<string>();

            _uniquePartOfUri = "cities/" + Country;

            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            if (response.IsSuccessStatusCode)
            {
                string jsonUsers = await response.Content.ReadAsStringAsync();
                _all = JsonConvert.DeserializeObject<List<string>>(jsonUsers);
                return _all;
            }
            throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }
    }
}
