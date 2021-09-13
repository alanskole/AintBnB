using AintBnB.App.Helpers;
using AintBnB.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AintBnB.App.Helpers.ApiCalls;
using static AintBnB.App.Helpers.UwpCookieHelper;

namespace AintBnB.App.ViewModels
{
    public class BookingViewModel : Observable
    {
        private string _startDate;
        private int _night;
        private string _uri = "booking/";
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

        public async Task BookAccommodationAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var uniquePartOfUri = "book";

                var bookingInfo = new string[] { StartDate, Booking.BookedBy.Id.ToString(), Nights.ToString(), Booking.Accommodation.Id.ToString() };

                await AddAuthCookieAsync(_clientProvider.clientHandler);

                await GetCsrfToken(_clientProvider);

                await PostAsync($"{_uri}{uniquePartOfUri}", bookingInfo, _clientProvider);
            }
        }

        public async Task UpdateBookingAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var uniquePartOfUri = Booking.Id;

                var newDates = new string[] { StartDate, Nights.ToString() };

                await AddAuthCookieAsync(_clientProvider.clientHandler);

                await GetCsrfToken(_clientProvider);

                await PutAsync($"{_uri}{uniquePartOfUri}", newDates, _clientProvider);

                await GetABookingAsync();
            }
        }

        public async Task RateAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var uniquePartOfUri = $"rate/{Booking.Id}";

                await AddAuthCookieAsync(_clientProvider.clientHandler);

                await GetCsrfToken(_clientProvider);

                await PutAsync($"{_uri}{uniquePartOfUri}", Booking.Rating, _clientProvider);

                await GetABookingAsync();
            }
        }

        public async Task GetABookingAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var uniquePartOfUri = Booking.Id;

                await AddAuthCookieAsync(_clientProvider.clientHandler);

                Booking = await GetAsync<Booking>($"{_uri}{uniquePartOfUri}", _clientProvider);
            }
        }

        public async Task GetAllBookingsAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                await AddAuthCookieAsync(_clientProvider.clientHandler);

                AllBookings = await GetAllAsync<Booking>(_uri, _clientProvider);
            }
        }

        public async Task GetAllBookingsOfOwnedAccommodationsAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var uniquePartOfUri = $"{UserId}/bookingsownaccommodation";

                await AddAuthCookieAsync(_clientProvider.clientHandler);

                AllBookings = await GetAllAsync<Booking>($"{_uri}{uniquePartOfUri}", _clientProvider);
            }
        }

        public async Task DeleteABookingAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var uniquePartOfUri = Booking.Id;

                await AddAuthCookieAsync(_clientProvider.clientHandler);

                await GetCsrfToken(_clientProvider);

                await DeleteAsync($"{_uri}{uniquePartOfUri}", _clientProvider);
            }
        }
    }
}
