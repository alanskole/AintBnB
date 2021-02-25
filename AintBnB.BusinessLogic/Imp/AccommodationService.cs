using AintBnB.Core.Models;
using static AintBnB.BusinessLogic.Helpers.DateHelper;
using static AintBnB.BusinessLogic.Helpers.Authentication;
using static AintBnB.BusinessLogic.Helpers.AllCountiresAndCitiesEurope;
using System;
using System.Collections.Generic;
using System.Linq;
using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Repository.Interfaces;
using System.Text.RegularExpressions;

namespace AintBnB.BusinessLogic.Imp
{
    public class AccommodationService : IAccommodationService
    {
        private IUnitOfWork _unitOfWork;

        public AccommodationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Accommodation CreateAccommodation(User owner, Address address, int squareMeters, int amountOfBedroooms, double kilometersFromCenter, string description, int pricePerNight, int cancellationDeadlineInDays, List<byte[]> picture, int daysToCreateScheduleFor)
        {
            if (CheckIfUserIsAllowedToPerformAction(owner))
            {
                Accommodation accommodation = new Accommodation(owner, address, squareMeters, amountOfBedroooms, kilometersFromCenter, description, pricePerNight, cancellationDeadlineInDays);

                accommodation.Picture = picture;

                if (daysToCreateScheduleFor < 1)
                    daysToCreateScheduleFor = 1;


                ValidateAccommodation(accommodation);

                CreateScheduleForXAmountOfDays(accommodation, daysToCreateScheduleFor);
                _unitOfWork.AccommodationRepository.Create(accommodation);
                _unitOfWork.Commit();
                return accommodation;
            }
            throw new AccessException($"Must be performed by a customer with ID {owner.Id}, or by admin or an employee on behalf of a customer with ID {owner.Id}!");
        }

        public void ValidateAccommodation(Accommodation accommodation)
        {
            accommodation.Description = accommodation.Description.Trim();
            accommodation.Address.Street = accommodation.Address.Street.Trim();
            accommodation.Address.Number = accommodation.Address.Number.Trim();
            accommodation.Address.Zip = accommodation.Address.Zip.Trim();
            accommodation.Address.City = accommodation.Address.City.Trim();
            accommodation.Address.Country = accommodation.Address.Country.Trim();

            if (accommodation.Owner.Id == 0)
                throw new ParameterException("Id", "zero");
            if (!onlyLettersNumbersOneSpaceOrDash.IsMatch(accommodation.Address.Street))
                throw new ParameterException("Street", "empty");
            if (!onlyNumbersFollowedByAnOptionalLetter.IsMatch(accommodation.Address.Number))
                throw new ParameterException("Number", "zero");
            if (!zipCodeFormatsOfTheWorld.IsMatch(accommodation.Address.Zip))
                throw new ParameterException("Zip", "empty");
            if (!onlyLettersNumbersOneSpaceOrDash.IsMatch(accommodation.Address.Area))
                throw new ParameterException("Area", "empty");
            IsCountryAndCityCorrect(accommodation.Address.Country, accommodation.Address.City);
            if (accommodation.SquareMeters == 0)
                throw new ParameterException("SquareMeters", "zero");
            if (accommodation.Description == null || accommodation.Description.Length == 0)
                throw new ParameterException("Description", "empty");
            if (accommodation.PricePerNight == 0)
                throw new ParameterException("PricePerNight", "zero");
            if (accommodation.CancellationDeadlineInDays < 1)
                throw new ParameterException("Cancellation deadline", "less than one day");
        }

        public Accommodation GetAccommodation(int id)
        {
            AnyoneLoggedIn();

            Accommodation acc = _unitOfWork.AccommodationRepository.Read(id);

            if (acc == null)
                throw new IdNotFoundException("Accommodation", id);

            return acc;
        }

        public List<Accommodation> GetAllAccommodations()
        {
            List<Accommodation> all = _unitOfWork.AccommodationRepository.GetAll();

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
            if (CorrectUserOrAdminOrEmployee(_unitOfWork.AccommodationRepository.Read(id).Owner))
            {
                GetAccommodation(id);

                accommodation.Description = accommodation.Description.Trim();

                ValidateUpdatedFields(accommodation.SquareMeters, accommodation.Description, accommodation.PricePerNight, accommodation.CancellationDeadlineInDays);

                accommodation.Id = id;

                _unitOfWork.AccommodationRepository.Update(id, accommodation);
                _unitOfWork.Commit();
            }
            else
                throw new AccessException($"Must be performed by a customer with ID {accommodation.Owner.Id}, or by admin or an employee on behalf of a customer with ID {accommodation.Owner.Id}!");
        }

        private static void ValidateUpdatedFields(int squareMeters, string description, int pricePerNight, int cancellationDeadlineInDays)
        {
            if (squareMeters == 0)
                throw new ParameterException("SquareMeters", "zero");
            if (description == null || description.Length == 0)
                throw new ParameterException("Description", "empty");
            if (pricePerNight == 0)
                throw new ParameterException("PricePerNight", "zero");
            if (cancellationDeadlineInDays < 1)
                throw new ParameterException("Cancellation deadline", "less than one day");
        }

        public void ExpandScheduleOfAccommodationWithXAmountOfDays(int id, int days)
        {
            User owner = _unitOfWork.AccommodationRepository.Read(id).Owner;

            if (CorrectUserOrAdminOrEmployee(owner))
            {
                SortedDictionary<string, bool> dateAndStatus = new SortedDictionary<string, bool>();

                DateTime fromDate = DateTime.Parse(_unitOfWork.AccommodationRepository.Read(id).Schedule.Keys.Last()).AddDays(1);

                addDaysToDate(days, fromDate, dateAndStatus);

                MergeTwoSortedDictionaries(_unitOfWork.AccommodationRepository.Read(id).Schedule, dateAndStatus);

                _unitOfWork.AccommodationRepository.Update(id, _unitOfWork.AccommodationRepository.Read(id));

                _unitOfWork.Commit();
            }
            else
                throw new AccessException($"Must be performed by a customer with ID {owner.Id}, or by admin or an employee on behalf of a customer with ID {owner.Id}!");
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

            List<Accommodation> available = new List<Accommodation>();

            SearchInCountryAndCity(country, city, startdate, nights, available);

            if (available.Count == 0)
                throw new DateException(($"No available accommodations found in {country}, {city} from {startdate} for {nights} nights"));

            return available;
        }

        private void SearchInCountryAndCity(string country, string city, string startdate, int nights, List<Accommodation> available)
        {
            foreach (Accommodation accommodation in _unitOfWork.AccommodationRepository.GetAll())
            {
                if (string.Equals(accommodation.Address.Country, country, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(accommodation.Address.City, city, StringComparison.OrdinalIgnoreCase))
                    AreDatesWithinRangeOfScheduleOfTheAccommodation(available, accommodation, startdate, nights);
            }
        }

        private void AreDatesWithinRangeOfScheduleOfTheAccommodation(List<Accommodation> availableOnes, Accommodation acm, string startDate, int nights)
        {
            if (AreAllDatesAvailable(acm.Schedule, startDate, nights))
                availableOnes.Add(acm);
        }

        public List<Accommodation> SortListOfAccommodations(List<Accommodation> available, string sortBy, string ascOrDesc)
        {
            if (available == null || available.Count == 0)
                throw new NoneFoundInDatabaseTableException("available accommodations");

            if (sortBy == "Price")
            {
                if (ascOrDesc == "Descending")
                    available.Sort((x, y) => y.PricePerNight.CompareTo(x.PricePerNight));
                else if (ascOrDesc == "Ascending")
                    available.Sort((x, y) => x.PricePerNight.CompareTo(y.PricePerNight));
            }
            else if (sortBy == "Distance")
            {
                if (ascOrDesc == "Descending")
                    available.Sort((x, y) => y.KilometersFromCenter.CompareTo(x.KilometersFromCenter));
                else if (ascOrDesc == "Ascending")
                    available.Sort((x, y) => x.KilometersFromCenter.CompareTo(y.KilometersFromCenter));
            }
            else if (sortBy == "Size")
            {
                if (ascOrDesc == "Descending")
                    available.Sort((x, y) => y.SquareMeters.CompareTo(x.SquareMeters));
                else if (ascOrDesc == "Ascending")
                    available.Sort((x, y) => x.SquareMeters.CompareTo(y.SquareMeters));
            }
            else if (sortBy == "Rating")
            {
                if (ascOrDesc == "Descending")
                    available.Sort((x, y) => y.AverageRating.CompareTo(x.AverageRating));
                else if (ascOrDesc == "Ascending")
                    available.Sort((x, y) => x.AverageRating.CompareTo(y.AverageRating));
            }

            return available;
        }
    }
}
