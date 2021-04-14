using AintBnB.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AintBnB.BusinessLogic.Interfaces
{
    public interface IBookingService
    {
        Task<Booking> GetBookingAsync(int id);
        Task<List<Booking>> GetAllBookingsAsync();
        Task<Booking> BookAsync(string startDate, User booker, int nights, Accommodation accommodation);
        Task<List<Booking>> GetBookingsOfOwnedAccommodationAsync(int userId);
        Task UpdateBookingAsync(string newStartDate, int nights, int bookingId);
        Task RateAsync(int bookingId, int rating);
    }
}
