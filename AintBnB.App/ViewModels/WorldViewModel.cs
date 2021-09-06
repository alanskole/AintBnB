using AintBnB.App.CommonMethodsAndProperties;
using AintBnB.App.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AintBnB.App.CommonMethodsAndProperties.ApiCalls;
using static AintBnB.App.Helpers.UwpCookieHelper;

namespace AintBnB.App.ViewModels
{
    public class WorldViewModel : Observable
    {
        private string _uri = "world/";
        private string _country;
        private string _city;
        private List<string> _allCountries;
        private List<string> _allCitiesOfACountry;


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

        public List<string> AllCountries
        {
            get { return _allCountries; }
            set
            {
                _allCountries = value;
                NotifyPropertyChanged("AllCountries");
            }
        }

        public List<string> AllCitiesOfACountry
        {
            get { return _allCitiesOfACountry; }
            set
            {
                _allCitiesOfACountry = value;
                NotifyPropertyChanged("AllCitiesOfACountry");
            }
        }

        public async Task GetAllCountriesInTheWorldAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var uniquePartOfUri = "countries";

                await AddAuthCookieAsync(_clientProvider.clientHandler);

                AllCountries = await GetAllAsync<string>(_uri + uniquePartOfUri, _clientProvider);
            }
        }

        public async Task GetAllCitiesOfACountryAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var uniquePartOfUri = "cities/" + Country;

                await AddAuthCookieAsync(_clientProvider.clientHandler);

                AllCitiesOfACountry = await GetAllAsync<string>(_uri + uniquePartOfUri, _clientProvider);
            }
        }
    }
}
