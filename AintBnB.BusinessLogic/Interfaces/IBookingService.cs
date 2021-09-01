using AintBnB.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AintBnB.BusinessLogic.Interfaces
{
    public interface IBookingService
    {
        /// <summary>Fetches a booking.</summary>
        /// <param name="id">The ID of the booking to get.</param>
        /// <returns>The booking object</returns>
        Task<Booking> GetBookingAsync(int id);

        /// <summary>Gets all the bookings in the database.</summary>
        /// <returns>A list of all the bookings</returns>
        Task<List<Booking>> GetAllInSystemAsync();

        /// <summary>Gets all bookings in the database belonging to a user.</summary>
        /// <param name="userid">The user-ID of the user to get the bookings of.</param>
        /// <returns>A list of all the bookings of the user</returns>
        Task<List<Booking>> GetOnlyOnesOwnedByUserAsync(int userId);

        /// <summary>Books an accommodation.</summary>
        /// <param name="startDate">The start date of the booking.</param>
        /// <param name="booker">The booker.</param>
        /// <param name="nights">The amount of nights to book for.</param>
        /// <param name="accommodation">The accommodation to book.</param>
        /// <returns>The booking object</returns>
        Task<Booking> BookAsync(string startDate, User booker, int nights, Accommodation accommodation);

        /// <summary>Gets all the bookings of a users accommodations.</summary>
        /// <param name="userid">The user-ID of the owner of the accommoations.</param>
        /// <returns>A list of all the booking objects</returns>
        Task<List<Booking>> GetBookingsOfOwnedAccommodationAsync(int userId);

        /// <summary>Updates the dates of an existing booking.</summary>
        /// <param name="newStartDate">The new start date.</param>
        /// <param name="nights">The amount of nights to book.</param>
        /// <param name="bookingId">The booking-ID of the booking to update.</param>
        Task UpdateBookingAsync(string newStartDate, int nights, int bookingId);

        /// <summary>Rates the accommodation that was booked after a booking has ended.</summary>
        /// <param name="bookingId">The booking-ID of the booking to leave the rating for.</param>
        /// <param name="rating">A rating between 1-5.</param>
        Task RateAsync(int bookingId, int rating);
    }
}
