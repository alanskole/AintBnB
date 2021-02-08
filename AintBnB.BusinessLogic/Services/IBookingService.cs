using AintBnB.Core.Models;
using System.Collections.Generic;

namespace AintBnB.BusinessLogic.Services
{
    public interface IBookingService
    {
        Booking GetBooking(int id);
        List<Booking> GetAllBookings();
        Booking Book(string startDate, User booker, int nights, Accommodation accommodation);
        List<Booking> GetBookingsOnOwnedAccommodation(int userId);
    }
}
