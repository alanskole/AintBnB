using AintBnB.Core.Models;
using AintBnB.BusinessLogic.DependencyProviderFactory;
using AintBnB.BusinessLogic.Repository;
using static AintBnB.BusinessLogic.Services.DateParser;
using static AintBnB.BusinessLogic.Services.UpdateScheduleInDatabase;
using static AintBnB.BusinessLogic.Services.AllCountiresAndCitiesEurope;
using System;
using System.Collections.Generic;
using System.Linq;
using AintBnB.BusinessLogic.CustomExceptions;

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

        public void ValidateAccommodation(Accommodation accommodation)
        {
            if (accommodation.Owner.Id == 0)
                throw new IdNotFoundException("User");
            if(accommodation.Address.Street == null || accommodation.Address.Street.Trim().Length == 0)
                throw new ParameterException("Street", "empty");
            if (accommodation.Address.Number == 0)
                throw new ParameterException("Number", "zero");
            if (accommodation.Address.Zip == 0)
                throw new ParameterException("Zip", "zero");
            if (accommodation.Address.Area == null || accommodation.Address.Area.Trim().Length == 0)
                throw new ParameterException("Area", "empty");
            IsCountryAndCityCorrect(accommodation.Address.Country.Trim(), accommodation.Address.City.Trim());
            if (accommodation.SquareMeters == 0)
                throw new ParameterException("SquareMeters", "zero");
            if (accommodation.Description == null || accommodation.Description.Trim().Length == 0)
                throw new ParameterException("Description", "empty");
            if (accommodation.PricePerNight == 0)
                throw new ParameterException("PricePerNight", "zero");
        }

        public Accommodation CreateAccommodation(User owner, Address address, int squareMeters, int amountOfBedroooms, double kilometersFromCenter, string description, int pricePerNight, int daysToCreateScheduleFor)
        {
            Accommodation accommodation = new Accommodation(owner, address, squareMeters, amountOfBedroooms, kilometersFromCenter, description, pricePerNight);

            if (daysToCreateScheduleFor < 1)
                daysToCreateScheduleFor = 1;
            
            try
            {
                ValidateAccommodation(accommodation);
            }
            catch (Exception)
            {
                throw;
            }

            CreateScheduleForXAmountOfDays(accommodation, daysToCreateScheduleFor);
            _iAccommodationRepository.Create(accommodation);
            return accommodation;
        }

        public Accommodation GetAccommodation(int id)
        {
            Accommodation acc = _iAccommodationRepository.Read(id);

            if (acc == null)
                throw new IdNotFoundException("Accommodation", id);

            return acc;
        }

        public List<Accommodation> GetAllAccommodations()
        {
            List<Accommodation> all = _iAccommodationRepository.GetAll();

            if (all.Count == 0)
                throw new NoneFoundInDatabaseTableException("accommodations");

            return all;
        }

        public void UpdateAccommodation(int id, Accommodation updatedAccommodation)
        {
            try
            {
                GetAccommodation(id);
                ValidateAccommodation(updatedAccommodation);
            }
            catch (Exception)
            {
                throw;
            }

            _iAccommodationRepository.Update(id, updatedAccommodation);
        }

        public void ExpandScheduleOfAccommodationWithXAmountOfDays(int id, int days)
        {
            SortedDictionary<string, bool> dateAndStatusOriginal = _iAccommodationRepository.Read(id).Schedule;
            SortedDictionary<string, bool> dateAndStatus = new SortedDictionary<string, bool>();

            DateTime fromDate = DateTime.Parse(dateAndStatusOriginal.Keys.Last()).AddDays(1);

            addDaysToDate(days, fromDate, dateAndStatus);

            MergeTwoSortedDictionaries(dateAndStatusOriginal, dateAndStatus);

            try
            {
                UpdateScheduleInDb(id, dateAndStatusOriginal);
            }
            catch (Exception)
            {
                throw;
            }
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

        public List<Accommodation> FindAvailable(string country, string city, string startdate, int nights)
        {
            List<Accommodation> availableOnes = new List<Accommodation>();

            SearchInCountryAndCity(country, city, startdate, nights, availableOnes);

            if (availableOnes.Count > 0)
                return availableOnes;
            else
                throw new DateException(($"No available accommodations found in {country}, {city} from {startdate} for {nights} nights"));
        }

        private void SearchInCountryAndCity(string country, string city, string startdate, int nights, List<Accommodation> availableOnes)
        {
            foreach (Accommodation accommodation in _iAccommodationRepository.GetAll())
            {
                if (string.Equals(accommodation.Address.Country, country, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(accommodation.Address.City, city, StringComparison.OrdinalIgnoreCase))
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
