using AintBnB.App.CommonMethodsAndProperties;
using AintBnB.App.Helpers;
using AintBnB.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AintBnB.App.CommonMethodsAndProperties.ApiCalls;
using static AintBnB.App.Helpers.UwpCookieHelper;

namespace AintBnB.App.ViewModels
{
    public class AccommodationViewModel : Observable
    {
        private HttpClientProvider _clientProvider = new HttpClientProvider();
        private string _uri;
        private string _uniquePartOfUri;
        private int _userId;
        private Accommodation _accommodation = new Accommodation { Address = new Address() };
        private int _daysSchedule;
        private int _expandScheduleByDays;
        private string _fromDate;
        private int _nights;
        private List<Accommodation> _allAccommodations;
        private string _sortBy = "";
        private string _ascOrDesc = "";
        public int UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                NotifyPropertyChanged("UserId");
            }
        }

        public Accommodation Accommodation
        {
            get { return _accommodation; }
            set
            {
                _accommodation = value;
                NotifyPropertyChanged("Accommodation");
            }
        }

        public int DaysSchedule
        {
            get { return _daysSchedule; }
            set
            {
                _daysSchedule = value;
                NotifyPropertyChanged("DaysSchedule");
            }
        }

        public int ExpandScheduleByDays
        {
            get { return _expandScheduleByDays; }
            set
            {
                _expandScheduleByDays = value;
                NotifyPropertyChanged("ExpandScheduleByDays");
            }
        }

        public string FromDate
        {
            get { return _fromDate; }
            set
            {
                _fromDate = value;
                NotifyPropertyChanged("FromDate");
            }
        }

        public int Nights
        {
            get { return _nights; }
            set
            {
                _nights = value;
                NotifyPropertyChanged("Nights");
            }
        }

        public List<Accommodation> AllAccommodations
        {
            get { return _allAccommodations; }
            set
            {
                _allAccommodations = value;
                NotifyPropertyChanged("AllAccommodations");
            }
        }

        public string SortBy
        {
            get { return _sortBy; }
            set
            {
                _sortBy = value;
                NotifyPropertyChanged("SortBy");
            }
        }

        public string AscOrDesc
        {
            get { return _ascOrDesc; }
            set
            {
                _ascOrDesc = value;
                NotifyPropertyChanged("AscOrDesc");
            }
        }

        public AccommodationViewModel()
        {
            _clientProvider.ControllerPartOfUri = "api/accommodation/";
            _uri = _clientProvider.LocalHostAddress + _clientProvider.LocalHostPort + _clientProvider.ControllerPartOfUri;
        }

        public async Task CreateAccommodationAsync()
        {
            _uniquePartOfUri = DaysSchedule.ToString() + "/" + UserId.ToString();

            await AddAuthCookieAsync(_clientProvider.clientHandler);

            await GetCsrfToken(_clientProvider);

            await PostAsync(_uri + _uniquePartOfUri, Accommodation, _clientProvider);
        }

        public async Task GetAccommodationAsync()
        {
            _uniquePartOfUri = Accommodation.Id.ToString();

            await AddAuthCookieAsync(_clientProvider.clientHandler);

            Accommodation = await GetAsync<Accommodation>(_uri + _uniquePartOfUri, _clientProvider);
        }

        public async Task GetAllAccommodationsAsync()
        {
            await AddAuthCookieAsync(_clientProvider.clientHandler);

            AllAccommodations = await GetAllAsync<Accommodation>(_uri, _clientProvider);
        }

        public async Task GetAllAccommodationsOfAUserAsync()
        {
            _uniquePartOfUri = UserId.ToString() + "/allaccommodations";

            await AddAuthCookieAsync(_clientProvider.clientHandler);

            AllAccommodations = await GetAllAsync<Accommodation>(_uri + _uniquePartOfUri, _clientProvider);
        }

        public async Task GetAvailableAsync()
        {
            _uniquePartOfUri = _accommodation.Address.Country + "/" + _accommodation.Address.City + "/" + FromDate + "/" + Nights.ToString();

            await AddAuthCookieAsync(_clientProvider.clientHandler);

            AllAccommodations = await GetAllAsync<Accommodation>(_uri + _uniquePartOfUri, _clientProvider);
        }

        public async Task SortAvailableListAsync()
        {
            _uniquePartOfUri = "sort/" + SortBy + "/" + AscOrDesc;

            await AddAuthCookieAsync(_clientProvider.clientHandler);

            AllAccommodations = await SortListAsync(_uri + _uniquePartOfUri, AllAccommodations, _clientProvider);
        }

        public async Task DeleteAccommodationAsync()
        {
            _uniquePartOfUri = Accommodation.Id.ToString();

            await AddAuthCookieAsync(_clientProvider.clientHandler);

            await GetCsrfToken(_clientProvider);

            await DeleteAsync(_uri + _uniquePartOfUri, _clientProvider);
        }

        public async Task UpdateAccommodationAsync()
        {
            _uniquePartOfUri = Accommodation.Id.ToString();

            await AddAuthCookieAsync(_clientProvider.clientHandler);

            await GetCsrfToken(_clientProvider);

            await PutAsync(_uri + _uniquePartOfUri, Accommodation, _clientProvider);
        }

        public async Task ExpandScheduleOfAccommodationAsync()
        {
            _uniquePartOfUri = Accommodation.Id.ToString() + "/" + ExpandScheduleByDays.ToString();

            await AddAuthCookieAsync(_clientProvider.clientHandler);

            await GetCsrfToken(_clientProvider);

            await GetAsync(_uri + _uniquePartOfUri, _clientProvider);
        }
    }
}
