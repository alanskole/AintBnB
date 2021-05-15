using AintBnB.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AintBnB.BusinessLogic.Interfaces
{
    public interface IAccommodationService
    {
        /// <summary>Validates the properties of an accommodation asynchronous.</summary>
        /// <param name="accommodation">The accommodation object to validate.</param>
        Task ValidateAccommodationAsync(Accommodation accommodation);

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
        Task<Accommodation> CreateAccommodationAsync(User owner, Address address, int squareMeters, int amountOfBedroooms, double kilometersFromCenter, string description, int pricePerNight, int cancellationDeadlineInDays, int daysToCreateScheduleFor);

        /// <summary>Fetches an accommodation from the database.</summary>
        /// <param name="id">The ID of the accommodation to fetch.</param>
        /// <returns>The accommodation object.</returns>
        Task<Accommodation> GetAccommodationAsync(int id);

        /// <summary>Gets a list of all the accommodations.</summary>
        /// <returns>A list with all the accommodation objects</returns>
        Task<List<Accommodation>> GetAllAccommodationsAsync();

        /// <summary>Gets all accommodations of a user.</summary>
        /// <param name="userid">The user-ID to get the accommodations of.</param>
        /// <returns>A list with all the accommodations of the user</returns>
        Task<List<Accommodation>> GetAllOwnedAccommodationsAsync(int userid);

        /// <summary>Updates an accommodation.</summary>
        /// <param name="id">The ID of the accommodation to update.</param>
        /// <param name="accommodation">The accommodation object with the updated values.</param>
        Task UpdateAccommodationAsync(int id, Accommodation accommodation);

        /// <summary>Expands the existing schedule of an accommodation with x amount of days.</summary>
        /// <param name="id">The ID of the accommodation.</param>
        /// <param name="days">The amount of days to expand the schedule by.</param>
        Task ExpandScheduleOfAccommodationWithXAmountOfDaysAsync(int id, int days);

        /// <summary>Finds the available accommodations in a city for a set of given dates.</summary>
        /// <param name="country">The country the accommodations must be located in.</param>
        /// <param name="city">The city the accommodations must be located in.</param>
        /// <param name="startdate">The startdate for the search.</param>
        /// <param name="nights">The amount of nights to search for.</param>
        Task<List<Accommodation>> FindAvailableAsync(string country, string city, string startdate, int nights);

        /// <summary>Sorts a list of accommodations.</summary>
        /// <param name="available">The list with the accommodations to sort.</param>
        /// <param name="sortBy">What to sort by; can be sorted by: Price, Distance, Size or Rating.</param>
        /// <param name="ascOrDesc">Sort in ascending or descending order.</param>
        /// <returns>A list with the accommodations that are sorted in the desired order</returns>
        List<Accommodation> SortListOfAccommodations(List<Accommodation> available, string sortBy, string ascOrDesc);
    }
}
