using AintBnB.Core.Models;
using AintBnB.Helpers;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using AintBnB.Services;
using System.Text;

namespace AintBnB.ViewModels
{
    public class BookingViewModel : Observable
    {
        private string _startDate;
        private int _bookerId;
        private int _night;
        private int _accommodationId;
        private HttpClientProvider _clientProvider = new HttpClientProvider();
        private string _uri;
        private string _uniquePartOfUri;
        private Booking _booking;
        private int _bookingId;

        public string StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                NotifyPropertyChanged("StartDate");
            }
        }

        public int BookerId
        {
            get { return _bookerId; }
            set
            {
                _bookerId = value;
                NotifyPropertyChanged("BookerId");
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

        public int AccommodationId
        {
            get { return _accommodationId; }
            set
            {
                _accommodationId = value;
                NotifyPropertyChanged("AccommodationId");
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

        public int BookingId
        {
            get {return _bookingId; }
            set
            {
                _bookingId = value;
                NotifyPropertyChanged("BookingId");
            }
        }

        public BookingViewModel()
        {
            _clientProvider.ControllerPartOfUri = "api/booking/";
            _uri = _clientProvider.LocalHostAddress + _clientProvider.LocalHostPort + _clientProvider.ControllerPartOfUri;
        }

        public async Task BookAccommodation()
        {
            _uniquePartOfUri = StartDate + "/" + BookerId.ToString() + "/" + Nights.ToString() + "/" + AccommodationId.ToString();

            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            if (response.IsSuccessStatusCode)
            {
                string jsonBooking = await response.Content.ReadAsStringAsync();
                Booking = JsonConvert.DeserializeObject<Booking>(jsonBooking);
                NotifyPropertyChanged("Booking");
            }
            else
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }

        public async Task GetABooking()
        {
            _uniquePartOfUri = BookingId.ToString();


            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            if (response.IsSuccessStatusCode)
            {
                string jsonBooking = await response.Content.ReadAsStringAsync();
                Booking = JsonConvert.DeserializeObject<Booking>(jsonBooking);
                NotifyPropertyChanged("Booking");
            }
            else
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }

        public async Task<List<Booking>> GetAllBookings()
        {
            List<Booking> all = new List<Booking>();

            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri));
            if (response.IsSuccessStatusCode)
            {
                string jsonBookings = await response.Content.ReadAsStringAsync();
                all = JsonConvert.DeserializeObject<List<Booking>>(jsonBookings);
                return all;
            }
            else
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }

        public async Task DeleteABooking()
        {
            _uniquePartOfUri = BookingId.ToString();

            HttpResponseMessage response = await _clientProvider.client.DeleteAsync(new Uri(_uri + _uniquePartOfUri));
            if (!response.IsSuccessStatusCode)
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }
    }
}
