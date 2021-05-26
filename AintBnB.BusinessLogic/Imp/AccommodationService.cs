using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Core.Models;
using AintBnB.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AintBnB.BusinessLogic.Helpers.AllCountriesAndCities;
using static AintBnB.BusinessLogic.Helpers.Authentication;
using static AintBnB.BusinessLogic.Helpers.DateHelper;
using static AintBnB.BusinessLogic.Helpers.Regexp;

namespace AintBnB.BusinessLogic.Imp
{
    public class AccommodationService : IAccommodationService
    {
        private IUnitOfWork _unitOfWork;

        public AccommodationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>Creates a new accommodation.</summary>
        /// <param name="owner">The owner of the accommodation.</param>
        /// <param name="address">The address of the accommodation.</param>
        /// <param name="squareMeters">How many square meters the accommodation is.</param>
        /// <param name="amountOfBedroooms">The amount of bedroooms the accommodation has.</param>
        /// <param name="kilometersFromCenter">The distance to the city center in kilometers.</param>
        /// <param name="description">A description of the accommodation.</param>
        /// <param name="pricePerNight">The nightly price.</param>
        /// <param name="cancellationDeadlineInDays">The cancellation deadline in days.</param>
        /// <param name="daysToCreateScheduleFor">How many days to create the schedule for.</param>
        /// <returns>The newly created accommodation object</returns>
        /// <exception cref="ParameterException">If the days to create the schedule for is less than one</exception>
        /// <exception cref="AccessException">If a non admin user tries to create an accommodation for another user than themselves</exception>
        public async Task<Accommodation> CreateAccommodationAsync(User owner, Address address, int squareMeters, int amountOfBedroooms, double kilometersFromCenter, string description, int pricePerNight, int cancellationDeadlineInDays, int daysToCreateScheduleFor)
        {
            if (daysToCreateScheduleFor < 1)
                throw new ParameterException("Days to create the schedule for", "less than one");

            if (CheckIfUserIsAllowedToPerformAction(owner))
            {
                var accommodation = new Accommodation(owner, address, squareMeters, amountOfBedroooms, kilometersFromCenter, description, pricePerNight, cancellationDeadlineInDays);

                await ValidateAccommodationAsync(accommodation);

                CreateScheduleForXAmountOfDays(accommodation, daysToCreateScheduleFor);
                await _unitOfWork.AccommodationRepository.CreateAsync(accommodation);
                await _unitOfWork.CommitAsync();
                return accommodation;
            }
            throw new AccessException($"Must be performed by a customer with ID {owner.Id}, or by admin on behalf of a customer with ID {owner.Id}!");
        }

        /// <summary>Validates the properties of an accommodation.</summary>
        /// <param name="accommodation">The accommodation object to validate.</param>
        /// <exception cref="ParameterException">
        /// Id is zero
        /// or
        /// Streetname contains any other characters than letters or numbers with a space or dash betwwen them
        /// or
        /// Number contains any other characters other than numbers, where the first number is larger than zero, ending by one optional letter
        /// or
        /// Zip contains any other characters than numbers, letters, space or dash between the numbers and letters
        /// or
        /// Area contains any other characters than letters or numbers with a space or dash betwwen them
        /// or
        /// SquareMeters is zero
        /// or
        /// Description is empty
        /// or
        /// PricePerNight is zero
        /// or
        /// Cancellation deadline is less than one day
        /// </exception>
        public async Task ValidateAccommodationAsync(Accommodation accommodation)
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
                throw new ParameterException("Street", "any other than letters or numbers with a space or dash betwwen them");
            if (!onlyNumbersFollowedByAnOptionalLetter.IsMatch(accommodation.Address.Number))
                throw new ParameterException("Number", "any other than numbers, where the first number is larger than zero, followed by one optional letter");
            if (!zipCodeFormatsOfTheWorld.IsMatch(accommodation.Address.Zip))
                throw new ParameterException("Zip", "any other than numbers, letters, space or dash between the numbers and letters");
            if (!onlyLettersNumbersOneSpaceOrDash.IsMatch(accommodation.Address.Area))
                throw new ParameterException("Area", "any other than letters or numbers with a space or dash betwwen them");
            await IsCountryAndCityCorrectAsync(accommodation.Address.Country, accommodation.Address.City);
            if (accommodation.SquareMeters == 0)
                throw new ParameterException("SquareMeters", "zero");
            if (accommodation.Description == null || accommodation.Description.Length == 0)
                throw new ParameterException("Description", "empty");
            if (accommodation.PricePerNight == 0)
                throw new ParameterException("PricePerNight", "zero");
            if (accommodation.CancellationDeadlineInDays < 1)
                throw new ParameterException("Cancellation deadline", "less than one day");
        }

        /// <summary>Creates the schedule for x amount of days for an accommodation.</summary>
        /// <param name="accommodation">The accommodation to create the schedule for.</param>
        /// <param name="days">The amount of days to create the schedule for.</param>
        private void CreateScheduleForXAmountOfDays(Accommodation accommodation, int days)
        {
            var todaysDate = DateTime.Today;
            var dateAndStatus = new SortedDictionary<string, bool>();
            AddDaysToDateAndAddToSchedule(days, todaysDate, dateAndStatus);
            accommodation.Schedule = dateAndStatus;
        }

        /// <summary>Adds x amount of days from a date to the schedule.</summary>
        /// <param name="days">The amount of days to add.</param>
        /// <param name="date">The start date that amount of days must be added to.</param>
        /// <param name="dateAndStatus">The sorted dictionary (schedule) with the dates and availability status.</param>
        private void AddDaysToDateAndAddToSchedule(int days, DateTime date, SortedDictionary<string, bool> dateAndStatus)
        {
            DateTime newDate;

            for (int i = 0; i < days; i++)
            {
                newDate = date.AddDays(i);
                dateAndStatus.Add(DateFormatterCustomDate(newDate), true);
            }
        }

        /// <summary>Fetches an accommodation from the database.</summary>
        /// <param name="id">The ID of the accommodation to fetch.</param>
        /// <returns>The accommodation object.</returns>
        /// <exception cref="IdNotFoundException">If the ID doesn't match any accommodation-IDs from the database</exception>
        public async Task<Accommodation> GetAccommodationAsync(int id)
        {
            AnyoneLoggedIn();

            var acc = await _unitOfWork.AccommodationRepository.ReadAsync(id);

            if (acc == null)
                throw new IdNotFoundException("Accommodation", id);

            return acc;
        }

        /// <summary>Gets a list of all the accommodations.</summary>
        /// <returns>A list with all the accommodation objects</returns>
        /// <exception cref="NoneFoundInDatabaseTableException">No accommodations found in the database</exception>
        public async Task<List<Accommodation>> GetAllAccommodationsAsync()
        {
            AnyoneLoggedIn();

            var all = await _unitOfWork.AccommodationRepository.GetAllAsync();

            if (all.Count == 0)
                throw new NoneFoundInDatabaseTableException("accommodations");

            return all;
        }

        /// <summary>Gets all accommodations of a user.</summary>
        /// <param name="userid">The user-ID to get the accommodations of.</param>
        /// <returns>A list with all the accommodations of the user</returns>
        /// <exception cref="NoneFoundInDatabaseTableException">The user doesn't have any accommodations</exception>
        public async Task<List<Accommodation>> GetAllOwnedAccommodationsAsync(int userid)
        {
            AnyoneLoggedIn();

            var all = new List<Accommodation>();

            await FindAllAccommodationsOfAUserAsync(all, userid);

            if (all.Count == 0)
                throw new NoneFoundInDatabaseTableException(userid, "accommodations");

            return all;
        }

        /// <summary>Finds all accommodations of a user.</summary>
        /// <param name="all">The list that will be filled with the accommodations of the user.</param>
        /// <param name="userid">The user-ID of the owner of the accommodations.</param>
        private async Task FindAllAccommodationsOfAUserAsync(List<Accommodation> all, int userid)
        {
            foreach (var acc in await GetAllAccommodationsAsync())
            {
                if (acc.Owner.Id == userid)
                    all.Add(acc);
            }
        }

        /// <summary>Updates an accommodation.</summary>
        /// <param name="id">The ID of the accommodation to update.</param>
        /// <param name="accommodation">The accommodation object with the updated values.</param>
        /// <exception cref="AccessException">If the user that tries to update the accommodation isn't the owner or admin</exception>
        public async Task UpdateAccommodationAsync(int id, Accommodation accommodation)
        {
            var acc = await _unitOfWork.AccommodationRepository.ReadAsync(id);

            var owner = acc.Owner;

            if (CorrectUserOrAdmin(owner.Id))
            {
                await GetAccommodationAsync(id);

                accommodation.Description = accommodation.Description.Trim();

                ValidateUpdatedFields(accommodation.SquareMeters, accommodation.Description, accommodation.PricePerNight, accommodation.CancellationDeadlineInDays);

                accommodation.Id = id;

                await _unitOfWork.AccommodationRepository.UpdateAsync(id, accommodation);
                await _unitOfWork.CommitAsync();
            }
            else
                throw new AccessException($"Must be performed by a customer with ID {owner.Id}, or by admin on behalf of a customer with ID {owner.Id}!");
        }

        /// <summary>Validates the updated accommodation properties.</summary>
        /// <param name="squareMeters">How many square meters the accommodation is.</param>
        /// <param name="description">The description of the accommodation.</param>
        /// <param name="pricePerNight">The nightly price.</param>
        /// <param name="cancellationDeadlineInDays">The cancellation deadline in days.</param>
        /// <exception cref="ParameterException">SquareMeters is zero
        /// or
        /// Description is empty
        /// or
        /// PricePerNight is zero
        /// or
        /// Cancellation deadline is less than one day</exception>
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

        /// <summary>Expands the existing schedule of an accommodation with x amount of days.</summary>
        /// <param name="id">The ID of the accommodation.</param>
        /// <param name="days">The amount of days to expand the schedule by.</param>
        /// <exception cref="ParameterException">Days is less than one</exception>
        /// <exception cref="AccessException">If the user that calls this method isn't the owner of the accommodation or admin</exception>
        public async Task ExpandScheduleOfAccommodationWithXAmountOfDaysAsync(int id, int days)
        {
            if (days < 1)
                throw new ParameterException("Days", "less than one");

            var acc = await GetAccommodationAsync(id);
            var owner = acc.Owner;

            if (CorrectUserOrAdmin(owner.Id))
            {
                var dateAndStatus = new SortedDictionary<string, bool>();

                var fromDate = DateTime.Parse(acc.Schedule.Keys.Last()).AddDays(1);

                AddDaysToDateAndAddToSchedule(days, fromDate, dateAndStatus);

                MergeTwoSortedDictionaries(acc.Schedule, dateAndStatus);

                await _unitOfWork.AccommodationRepository.UpdateAsync(id, acc);

                await _unitOfWork.CommitAsync();
            }
            else
                throw new AccessException($"Must be performed by a customer with ID {owner.Id}, or by admin on behalf of a customer with ID {owner.Id}!");
        }

        /// <summary>Merges a newly created sorted dictionarie with the orignal one to exapnd the schedule of an accommodation.</summary>
        /// <param name="dateAndStatusOriginal">The original sorted dictionary (schedule) that must be expanded.</param>
        /// <param name="dateAndStatus">The new sorted dictionary (schedule) that must be added to the original one.</param>
        private static void MergeTwoSortedDictionaries(SortedDictionary<string, bool> dateAndStatusOriginal, SortedDictionary<string, bool> dateAndStatus)
        {
            foreach (var values in dateAndStatus)
            {
                dateAndStatusOriginal.Add(values.Key, values.Value);
            }
        }

        /// <summary>Finds the available accommodations in a city for a set of given dates.</summary>
        /// <param name="country">The country the accommodations must be located in.</param>
        /// <param name="city">The city the accommodations must be located in.</param>
        /// <param name="startdate">The startdate for the search.</param>
        /// <param name="nights">The amount of nights to search for.</param>
        /// <returns>A list with all the accommodations that satisfy the search criteria</returns>
        /// <exception cref="DateException">No available accommodations found that satisfy the search</exception>
        public async Task<List<Accommodation>> FindAvailableAsync(string country, string city, string startdate, int nights)
        {
            AnyoneLoggedIn();

            var available = new List<Accommodation>();

            await SearchInCountryAndCityAsync(country, city, startdate, nights, available);

            if (available.Count == 0)
                throw new DateException(($"No available accommodations found in {country}, {city} from {startdate} for {nights} nights"));

            return available;
        }

        /// <summary>Searches the in country and city for accommodations.</summary>
        /// <param name="country">The country the accommodations must be located in.</param>
        /// <param name="city">The city the accommodations must be located in.</param>
        /// <param name="startdate">The startdate for the search.</param>
        /// <param name="nights">The amount of nights to search for.</param>
        /// <param name="available">The list to add available accommodations that match the search criteria to.</param>
        private async Task SearchInCountryAndCityAsync(string country, string city, string startdate, int nights, List<Accommodation> available)
        {
            foreach (var accommodation in await _unitOfWork.AccommodationRepository.GetAllAsync())
            {
                if (string.Equals(accommodation.Address.Country, country, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(accommodation.Address.City, city, StringComparison.OrdinalIgnoreCase))
                    AreDatesWithinRangeOfScheduleOfTheAccommodation(available, accommodation, startdate, nights);
            }
        }

        /// <summary>Ares the dates that are searched for within range of schedule of the accommodation.</summary>
        /// <param name="availableOnes">The list to add available accommodations that match the search criteria to.</param>
        /// <param name="acm">The accommodation to check the schedule of.</param>
        /// <param name="startDate">The start date to search for.</param>
        /// <param name="nights">The amount of nights to search for.</param>
        private void AreDatesWithinRangeOfScheduleOfTheAccommodation(List<Accommodation> availableOnes, Accommodation acm, string startDate, int nights)
        {
            if (AreAllDatesAvailable(acm.Schedule, startDate, nights))
                availableOnes.Add(acm);
        }

        /// <summary>Sorts a list of accommodations.</summary>
        /// <param name="available">The list with the accommodations to sort.</param>
        /// <param name="sortBy">What to sort by; can be sorted by: Price, Distance, Size or Rating.</param>
        /// <param name="ascOrDesc">Sort in ascending or descending order.</param>
        /// <returns>A list with the accommodations that are sorted in the desired order</returns>
        /// <exception cref="NoneFoundInDatabaseTableException">If the list of accommodations are empty</exception>
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
