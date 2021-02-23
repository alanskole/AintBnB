using AintBnB.Core.Models;
using System.Collections.Generic;

namespace AintBnB.BusinessLogic.Interfaces
{
    public interface IBookingService
    {
        Booking GetBooking(int id);
        List<Booking> GetAllBookings();
        Booking Book(string startDate, User booker, int nights, Accommodation accommodation);
        List<Booking> GetBookingsOfOwnedAccommodation(int userId);
        void UpdateBooking(string newStartDate, int nights, int bookingId);
        void Rate(int bookingId, int rating);
    }
}
