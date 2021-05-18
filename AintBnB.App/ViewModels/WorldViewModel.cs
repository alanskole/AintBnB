using AintBnB.App.CommonMethodsAndProperties;
using AintBnB.App.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AintBnB.App.CommonMethodsAndProperties.ApiCalls;

namespace AintBnB.App.ViewModels
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

        public async Task<List<string>> GetAllCountriesInTheWorldAsync()
        {
            _uniquePartOfUri = "countries";

            return await GetAllAsync<string>(_uri + _uniquePartOfUri, _clientProvider);
        }

        public async Task<List<string>> GetAllCitiesOfACountryAsync()
        {
            _uniquePartOfUri = "cities/" + Country;

            return await GetAllAsync<string>(_uri + _uniquePartOfUri, _clientProvider);
        }
    }
}
