using AintBnB.Core.Models;
using AintBnB.BusinessLogic.DependencyProviderFactory;
using AintBnB.BusinessLogic.Repository;
using static AintBnB.BusinessLogic.Services.DateParser;
using static AintBnB.BusinessLogic.Services.UpdateScheduleInDatabase;
using static AintBnB.BusinessLogic.Services.AuthenticationService;
using System;
using System.Collections.Generic;
using AintBnB.BusinessLogic.CustomExceptions;

namespace AintBnB.BusinessLogic.Services
{
    public class BookingService : IBookingService
    {
        private IRepository<Booking> _iBookingRepository;

        public IRepository<Booking> IBookingRepository
        {
            get { return _iBookingRepository; }
            set
            {
                if (value == null)
                    throw new ArgumentException("IBookingRepository cannot be null");
                _iBookingRepository = value;
            }
        }

        public BookingService()
        {
            _iBookingRepository = ProvideDependencyFactory.bookingRepository;
        }

        public BookingService(IRepository<Booking> bookingRepo)
        {
            _iBookingRepository = bookingRepo;

        }

        public Booking GetBooking(int id)
        {
            try
            {
                CorrectUserOrOwner(_iBookingRepository.Read(id).Accommodation.Owner.Id, _iBookingRepository.Read(id).BookedBy.Id);
            }
            catch (Exception)
            {

                throw;
            }

            Booking booking = _iBookingRepository.Read(id);

            if (booking == null)
                throw new IdNotFoundException("Booking", id);

            return booking;
        }

        public List<Booking> GetBookingsOnOwnedAccommodation(int userid)
        {
            try
            {
                AnyoneLoggedIn();
            }
            catch (Exception)
            {
                throw;
            }

            List<Booking> bookingsOfOwnedAccommodation = new List<Booking>();

            foreach (var booking in _iBookingRepository.GetAll())
            {
                if (booking.Accommodation.Owner.Id == userid)
                    bookingsOfOwnedAccommodation.Add(booking);
            }

            if (bookingsOfOwnedAccommodation.Count == 0)
                throw new NoneFoundInDatabaseTableException(userid, "bookings of owned accommodations");

            return bookingsOfOwnedAccommodation;
        }

        public List<Booking> GetAllBookings()
        {
            try
            {
                AnyoneLoggedIn();
            }
            catch (Exception)
            {
                throw;
            }

            if (LoggedInAs.UserType == UserTypes.Admin)
            {
                try
                {
                    return GetAllInSystem();

                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    return GetOnlyOnesOwnedByUser();
                }
                catch (Exception)
                {
                    throw;
                }
            }
            
        }

        private List<Booking> GetOnlyOnesOwnedByUser()
        {
            List<Booking> bookingsOfLoggedInUser = new List<Booking>();

            foreach (var booking in _iBookingRepository.GetAll())
            {
                if (booking.BookedBy.Id == LoggedInAs.Id)
                {
                    bookingsOfLoggedInUser.Add(booking);
                }
            }

            if (bookingsOfLoggedInUser.Count == 0)
                throw new NoneFoundInDatabaseTableException(LoggedInAs.Id, "bookings");

            return bookingsOfLoggedInUser;
        }

        private List<Booking> GetAllInSystem()
        {
            List<Booking> all = _iBookingRepository.GetAll();

            if (all.Count == 0)
                throw new NoneFoundInDatabaseTableException("bookings");

            return all;
        }

        public Booking Book(string startDate, User booker, int nights, Accommodation accommodation)
        {
            try
            {
                CorrectUser(booker.Id);
            }
            catch (Exception)
            {
                throw;
            }

            if (nights < 1)
                throw new ParameterException("Nights", "less than one");

            startDate = startDate.Trim();

            if (AreAllDatesAvailable(accommodation.Schedule, startDate, nights))
            {
                List<string> datesBooked = new List<string>();
                AddDatesToList(datesBooked, startDate, nights);
                
                try
                {
                    SetStatusToUnavailable(accommodation, datesBooked);
                }
                catch (Exception)
                {

                    throw;
                }

                int totalPrice = nights * accommodation.PricePerNight;
                Booking booking = new Booking(booker, accommodation, datesBooked, totalPrice);
                _iBookingRepository.Create(booking);
                return booking;
            }
            else
                throw new DateException("Dates aren't available");
        }

        private void AddDatesToList(List<string> datesBooked, string startDate, int nights)
        {
            for (int i = 0; i < nights; i++)
            {
                string dateToAdd = DateFormatterCustomDate(DateTime.Parse(startDate).AddDays(i));
                datesBooked.Add(dateToAdd);
            }
        }

        private void SetStatusToUnavailable(Accommodation accommodation, List<string> datesBooked)
        {
            for (int i = 0; i < datesBooked.Count; i++)
                accommodation.Schedule[datesBooked[i]] = false;

            try
            {
                UpdateScheduleInDb(accommodation.Id, accommodation.Schedule);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
