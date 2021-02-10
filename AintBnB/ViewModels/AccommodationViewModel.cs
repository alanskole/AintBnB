using AintBnB.Core.Models;
using AintBnB.Helpers;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using AintBnB.Services;
using System.Text;
using Windows.UI.Xaml.Media.Imaging;

namespace AintBnB.ViewModels
{
    public class AccommodationViewModel : Observable
    {
        private HttpClientProvider _clientProvider = new HttpClientProvider();
        private string _uri;
        private string _uniquePartOfUri;
        private int _userId;
        private Accommodation _accommodation = new Accommodation { Address = new Address(), Picture = new List<byte[]>()};
        private int _daysSchedule;
        private int _expandScheduleByDays;
        private string _fromDate;
        private int _nights;

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
            get {return _daysSchedule; }
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

        public AccommodationViewModel()
        {
            _clientProvider.ControllerPartOfUri = "api/accommodation/";
            _uri = _clientProvider.LocalHostAddress + _clientProvider.LocalHostPort + _clientProvider.ControllerPartOfUri;
        }

        public async Task CreateAccommodation()
        {
            _uniquePartOfUri = DaysSchedule.ToString() + "/" + UserId.ToString();
            string accJson = JsonConvert.SerializeObject(Accommodation);
            HttpResponseMessage response = await _clientProvider.client.PostAsync(
                new Uri(_uri + _uniquePartOfUri), new StringContent(accJson, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }

        public async Task GetAccommodation()
        {
            _uniquePartOfUri = Accommodation.Id.ToString();


            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            if (response.IsSuccessStatusCode)
            {
                string jsonAcc = await response.Content.ReadAsStringAsync();
                _accommodation = JsonConvert.DeserializeObject<Accommodation>(jsonAcc);
                NotifyPropertyChanged("Accommodation");
            }
            else
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }

        public async Task<List<Accommodation>> GetAllAccommodations()
        {
            List<Accommodation> all = new List<Accommodation>();
    
            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri));
            if (response.IsSuccessStatusCode)
            {
                string jsonAccList = await response.Content.ReadAsStringAsync();
                all = JsonConvert.DeserializeObject<List<Accommodation>>(jsonAccList);
                return all;
            }
            else
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }

        public async Task<List<Accommodation>> GetAllAccommodationsOfAUser()
        {
            List<Accommodation> all = new List<Accommodation>();

            _uniquePartOfUri = UserId.ToString() + "/allaccommodations";

            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            if (response.IsSuccessStatusCode)
            {
                string jsonAccList = await response.Content.ReadAsStringAsync();
                all = JsonConvert.DeserializeObject<List<Accommodation>>(jsonAccList);
                return all;
            }
            else
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }

        public async Task<List<Accommodation>> GetAvailable()
        {
            List<Accommodation> _all = new List<Accommodation>();

            _uniquePartOfUri = _accommodation.Address.Country + "/" + _accommodation.Address.City + "/" + FromDate + "/" + Nights.ToString();

            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            if (response.IsSuccessStatusCode)
            {
                string jsonAcc = await response.Content.ReadAsStringAsync();
                _all = JsonConvert.DeserializeObject<List<Accommodation>>(jsonAcc);
                return _all;
            }
            throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }

        public async Task DeleteAccommodation()
        {
            _uniquePartOfUri = Accommodation.Id.ToString();

            HttpResponseMessage response = await _clientProvider.client.DeleteAsync(new Uri(_uri + _uniquePartOfUri));
            if (!response.IsSuccessStatusCode)
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }

        public async Task UpdateAccommodation()
        {
            _uniquePartOfUri = Accommodation.Id.ToString();

            string accJson = JsonConvert.SerializeObject(Accommodation);

            HttpResponseMessage response = await _clientProvider.client.PutAsync(
                new Uri(_uri + _uniquePartOfUri), new StringContent(accJson, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }

        public async Task ExpandScheduleOfAccommodation()
        {
            _uniquePartOfUri = Accommodation.Id.ToString() + "/" + ExpandScheduleByDays.ToString();

            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));
            if (!response.IsSuccessStatusCode)
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }
    }
}
