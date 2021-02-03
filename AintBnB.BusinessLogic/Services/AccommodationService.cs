using AintBnB.Core.Models;
using AintBnB.BusinessLogic.DependencyProviderFactory;
using AintBnB.BusinessLogic.Repository;
using static AintBnB.BusinessLogic.Services.DateParser;
using static AintBnB.BusinessLogic.Services.UpdateScheduleInDatabase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AintBnB.BusinessLogic.Services
{
    public class AccommodationService : IAccommodationService
    {
        private IRepository<Accommodation> _iAccommodationRepository;

        public IRepository<Accommodation> IAccommodationRepository
        {
            get { return _iAccommodationRepository; }
            set
            {
                if (value == null)
                    throw new ArgumentException("IAccommodationRepository cannot be null");
                _iAccommodationRepository = value;
            }
        }

        public AccommodationService()
        {
            _iAccommodationRepository = ProvideDependencyFactory.accommodationRepository;
        }

        public AccommodationService(IRepository<Accommodation> accommodationRepo)
        {
            _iAccommodationRepository = accommodationRepo;
        }

        public Accommodation CreateAccommodation(User owner, Address address, int squareMeters, int amountOfBedroooms, double kilometersFromCenter, string description, int pricePerNight, int daysToCreateScheduleFor)
        {
            Accommodation accommodation = new Accommodation(owner, address, squareMeters, amountOfBedroooms, kilometersFromCenter, description, pricePerNight);
            CreateScheduleForXAmountOfDays(accommodation, daysToCreateScheduleFor);
            _iAccommodationRepository.Create(accommodation);
            return accommodation;
        }

        public Accommodation GetAccommodation(int id)
        {
            return _iAccommodationRepository.Read(id);
        }

        public List<Accommodation> GetAllAccommodations()
        {
            return _iAccommodationRepository.GetAll();
        }

        public void UpdateAccommodation(int id, Accommodation updatedAccommodation)
        {
            _iAccommodationRepository.Update(id, updatedAccommodation);
        }
        
        public void ExpandScheduleOfAccommodationWithXAmountOfDays(int id, int days)
        {
            if (days < 1)
            {
                days = 1;
            }


            SortedDictionary<string, bool> dateAndStatusOriginal = _iAccommodationRepository.Read(id).Schedule;
            SortedDictionary<string, bool> dateAndStatus = new SortedDictionary<string, bool>();
            
            DateTime fromDate = DateTime.Parse(dateAndStatusOriginal.Keys.Last()).AddDays(1);

            addDaysToDate(days, fromDate, dateAndStatus);

            MergeTwoSortedDictionaries(dateAndStatusOriginal, dateAndStatus);

            UpdateScheduleInDb(id, dateAndStatusOriginal);
        }

        private static void MergeTwoSortedDictionaries(SortedDictionary<string, bool> dateAndStatusOriginal, SortedDictionary<string, bool> dateAndStatus)
        {
            foreach (var values in dateAndStatus)
            {
                dateAndStatusOriginal.Add(values.Key, values.Value);
            }
        }

        private void CreateScheduleForXAmountOfDays(Accommodation accommodation, int days)
        {
            if (days < 1)
            {
                days = 1;
            }

            DateTime todaysDate = DateTime.Today;
            SortedDictionary<string, bool> dateAndStatus = new SortedDictionary<string, bool>();
            addDaysToDate(days, todaysDate, dateAndStatus);
            accommodation.Schedule = dateAndStatus;
        }

        private void addDaysToDate(int days, DateTime date, SortedDictionary<string, bool> dateAndStatus)
        {
            DateTime newDate;

            for (int i = 0; i < days; i++)
            {
                newDate = date.AddDays(i);
                dateAndStatus.Add(DateFormatterCustomDate(newDate), true);

            }
        }

        public List<Accommodation> FindAvailable(string country, string municipality, string startdate, int nights)
        {
            List<Accommodation> availableOnes = new List<Accommodation>();

            SearchInCountryAndMunicipality(country, municipality, startdate, nights, availableOnes);

            if (availableOnes.Count > 0)
                return availableOnes;
            else
                throw new ArgumentException("Couldn't find any available dates!");
        }

        private void SearchInCountryAndMunicipality(string country, string municipality, string startdate, int nights, List<Accommodation> availableOnes)
        {
            foreach (Accommodation accommodation in _iAccommodationRepository.GetAll())
            {
                if (string.Equals(accommodation.Address.Country, country, StringComparison.OrdinalIgnoreCase) && 
                    string.Equals(accommodation.Address.City, municipality, StringComparison.OrdinalIgnoreCase))
                    AreDatesWithinRangeOfScheduleOfTheAccommodation(availableOnes, accommodation, startdate, nights);
            }
        }

        private void AreDatesWithinRangeOfScheduleOfTheAccommodation(List<Accommodation> availableOnes, Accommodation acm, string startDate, int nights)
        {
            if (AreAllDatesAvailable(acm.Schedule, startDate, nights))
                availableOnes.Add(acm);
        }
    }
}
