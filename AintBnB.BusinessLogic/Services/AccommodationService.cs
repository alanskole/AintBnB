using AintBnB.Core.Models;
using AintBnB.BusinessLogic.DependencyProviderFactory;
using AintBnB.BusinessLogic.Repository;
using static AintBnB.BusinessLogic.Services.DateService;
using static AintBnB.BusinessLogic.Services.UpdateScheduleInDatabase;
using static AintBnB.BusinessLogic.Services.AllCountiresAndCitiesEurope;
using static AintBnB.BusinessLogic.Services.AuthenticationService;
using System;
using System.Collections.Generic;
using System.Linq;
using AintBnB.BusinessLogic.CustomExceptions;

namespace AintBnB.BusinessLogic.Services
{
    public class AccommodationService : IAccommodationService
    {
        private IRepository<Accommodation> _iAccommodationRepository;
        private List<Accommodation> _available;

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

        public Accommodation CreateAccommodation(User owner, Address address, int squareMeters, int amountOfBedroooms, double kilometersFromCenter, string description, int pricePerNight, int cancellationDeadlineInDays, List<byte[]> picture, int daysToCreateScheduleFor)
        {
            if (CheckIfUserIsAllowedToPerformAction(owner.Id))
            {
                Accommodation accommodation = new Accommodation(owner, address, squareMeters, amountOfBedroooms, kilometersFromCenter, description, pricePerNight, cancellationDeadlineInDays);

                accommodation.Picture = picture;

                if (daysToCreateScheduleFor < 1)
                    daysToCreateScheduleFor = 1;

                ValidateAccommodation(accommodation);

                CreateScheduleForXAmountOfDays(accommodation, daysToCreateScheduleFor);
                _iAccommodationRepository.Create(accommodation);
                return accommodation;
            }
            throw new AccessException($"Must be performed by a customer with ID {owner.Id}, or by admin or an employee on behalf of a customer with ID {owner.Id}!");
        }

        public void ValidateAccommodation(Accommodation accommodation)
        {
            if (accommodation.Owner.Id == 0)
                throw new IdNotFoundException("User");
            if (accommodation.Address.Street == null || accommodation.Address.Street.Trim().Length == 0)
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
            if (accommodation.CancellationDeadlineInDays < 1)
                throw new ParameterException("Cancellation deadline", "less than one day");
        }

        public Accommodation GetAccommodation(int id)
        {
            AnyoneLoggedIn();

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

        public List<Accommodation> GetAllOwnedAccommodations(int userid)
        {
            List<Accommodation> all = new List<Accommodation>();

            FindAllAccommodationsOfAUser(all, userid);

            if (all.Count == 0)
                throw new NoneFoundInDatabaseTableException(userid, "accommodations");

            return all;
        }

        private void FindAllAccommodationsOfAUser(List<Accommodation> all, int userid)
        {
            foreach (var acc in GetAllAccommodations())
            {
                if (acc.Owner.Id == userid)
                    all.Add(acc);
            }
        }

        public void UpdateAccommodation(int id, Accommodation accommodation)
        {
            if (CorrectUserOrAdminOrEmployee(_iAccommodationRepository.Read(id).Owner.Id))
            {
                GetAccommodation(id);
                ValidateUpdatedFields(accommodation.SquareMeters, accommodation.Description, accommodation.PricePerNight, accommodation.CancellationDeadlineInDays);

                accommodation.Id = id;

                _iAccommodationRepository.Update(id, accommodation);
            }
            else
                throw new AccessException($"Must be performed by a customer with ID {accommodation.Owner.Id}, or by admin or an employee on behalf of a customer with ID {accommodation.Owner.Id}!");
        }

        private static void ValidateUpdatedFields(int squareMeters, string description, int pricePerNight, int cancellationDeadlineInDays)
        {
            if (squareMeters == 0)
                throw new ParameterException("SquareMeters", "zero");
            if (description == null || description.Trim().Length == 0)
                throw new ParameterException("Description", "empty");
            if (pricePerNight == 0)
                throw new ParameterException("PricePerNight", "zero");
            if (cancellationDeadlineInDays < 1)
                throw new ParameterException("Cancellation deadline", "less than one day");
        }

        public void ExpandScheduleOfAccommodationWithXAmountOfDays(int id, int days)
        {
            int ownerId = _iAccommodationRepository.Read(id).Owner.Id;

            if (CorrectUserOrAdminOrEmployee(ownerId))
            {
                SortedDictionary<string, bool> dateAndStatusOriginal = _iAccommodationRepository.Read(id).Schedule;
                SortedDictionary<string, bool> dateAndStatus = new SortedDictionary<string, bool>();

                DateTime fromDate = DateTime.Parse(dateAndStatusOriginal.Keys.Last()).AddDays(1);

                addDaysToDate(days, fromDate, dateAndStatus);

                MergeTwoSortedDictionaries(dateAndStatusOriginal, dateAndStatus);

                UpdateScheduleInDb(id, dateAndStatusOriginal);
            }
            else
                throw new AccessException($"Must be performed by a customer with ID {ownerId}, or by admin or an employee on behalf of a customer with ID {ownerId}!");
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
            AnyoneLoggedIn();

            _available = new List<Accommodation>();

            SearchInCountryAndCity(country, city, startdate, nights, _available);

            if (_available.Count == 0)
                throw new DateException(($"No available accommodations found in {country}, {city} from {startdate} for {nights} nights"));

            return _available;
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

        public List<Accommodation> SortListOfAccommodations(string sortBy, string ascOrDesc)
        {
            if (_available == null || _available.Count == 0)
                throw new NoneFoundInDatabaseTableException("available accommodations");

            if (sortBy == "Price")
            {
                if (ascOrDesc == "Descending")
                    _available.Sort((x, y) => y.PricePerNight.CompareTo(x.PricePerNight));
                else if (ascOrDesc == "Ascending")
                    _available.Sort((x, y) => x.PricePerNight.CompareTo(y.PricePerNight));
            }
            else if (sortBy == "Distance")
            {
                if (ascOrDesc == "Descending")
                    _available.Sort((x, y) => y.KilometersFromCenter.CompareTo(x.KilometersFromCenter));
                else if (ascOrDesc == "Ascending")
                    _available.Sort((x, y) => x.KilometersFromCenter.CompareTo(y.KilometersFromCenter));
            }
            else if (sortBy == "Size")
            {
                if (ascOrDesc == "Descending")
                    _available.Sort((x, y) => y.SquareMeters.CompareTo(x.SquareMeters));
                else if (ascOrDesc == "Ascending")
                    _available.Sort((x, y) => x.SquareMeters.CompareTo(y.SquareMeters));
            }

            return _available;
        }
    }
}
