using AintBnB.App.CommonMethodsAndProperties;
using AintBnB.App.Helpers;
using AintBnB.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AintBnB.App.CommonMethodsAndProperties.ApiCalls;
using static AintBnB.App.Helpers.UwpCookieHelper;

namespace AintBnB.App.ViewModels
{
    public class BookingViewModel : Observable
    {
        private string _startDate;
        private int _night;
        private HttpClientProvider _clientProvider = new HttpClientProvider();
        private string _uri;
        private string _uniquePartOfUri;
        private Booking _booking = new Booking { BookedBy = new User(), Accommodation = new Accommodation(), Dates = new List<string>() };
        private int _userId;
        List<Booking> _allBookings;

        public string StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                NotifyPropertyChanged("StartDate");
            }
        }

        public int Nights
        {
            get { return _night; }
            set
            {
                _night = value;
                NotifyPropertyChanged("Nights");
            }
        }

        public Booking Booking
        {
            get { return _booking; }
            set
            {
                _booking = value;
                NotifyPropertyChanged("Booking");
            }
        }

        public int UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                NotifyPropertyChanged("UserId");
            }
        }

        public List<Booking> AllBookings
        {
            get { return _allBookings; }
            set
            {
                _allBookings = value;
                NotifyPropertyChanged("AllBookings");
            }
        }

        public BookingViewModel()
        {
            _clientProvider.ControllerPartOfUri = "api/booking/";
            _uri = _clientProvider.LocalHostAddress + _clientProvider.LocalHostPort + _clientProvider.ControllerPartOfUri;
        }

        public async Task BookAccommodationAsync()
        {
            _uniquePartOfUri = StartDate + "/" + Booking.BookedBy.Id + "/" + Nights + "/" + Booking.Accommodation.Id;

            await AddAuthCookieAsync(_clientProvider.clientHandler);

            await GetCsrfToken(_clientProvider);

            Booking = await GetAsync<Booking>(_uri + _uniquePartOfUri, _clientProvider);
        }

        public async Task UpdateBookingAsync()
        {
            _uniquePartOfUri = StartDate + "/" + Nights + "/" + Booking.Id;

            await AddAuthCookieAsync(_clientProvider.clientHandler);

            await GetCsrfToken(_clientProvider);

            Booking = await GetAsync<Booking>(_uri + _uniquePartOfUri, _clientProvider);
        }

        public async Task RateAsync()
        {
            _uniquePartOfUri = "rate/" + Booking.Id + "/" + Booking.Rating;

            await AddAuthCookieAsync(_clientProvider.clientHandler);

            await GetCsrfToken(_clientProvider);

            await GetAsync(_uri + _uniquePartOfUri, _clientProvider);
        }

        public async Task GetABookingAsync()
        {
            _uniquePartOfUri = Booking.Id.ToString();

            await AddAuthCookieAsync(_clientProvider.clientHandler);

            Booking = await GetAsync<Booking>(_uri + _uniquePartOfUri, _clientProvider);
        }

        public async Task GetAllBookingsAsync()
        {
            await AddAuthCookieAsync(_clientProvider.clientHandler);

            AllBookings = await GetAllAsync<Booking>(_uri, _clientProvider);
        }

        public async Task GetAllBookingsOfOwnedAccommodationsAsync()
        {
            _uniquePartOfUri = UserId + "/" + "bookingsownaccommodation";

            await AddAuthCookieAsync(_clientProvider.clientHandler);

            AllBookings = await GetAllAsync<Booking>(_uri + _uniquePartOfUri, _clientProvider);
        }

        public async Task DeleteABookingAsync()
        {
            _uniquePartOfUri = Booking.Id.ToString();

            await AddAuthCookieAsync(_clientProvider.clientHandler);

            await GetCsrfToken(_clientProvider);

            await DeleteAsync(_uri + _uniquePartOfUri, _clientProvider);
        }
    }
}
