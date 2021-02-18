using AintBnB.Core.Models;
using AintBnB.BusinessLogic.DependencyProviderFactory;
using AintBnB.BusinessLogic.Repository;
using static AintBnB.BusinessLogic.Services.DateService;
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
            if (CorrectUserOrOwnerOrAdminOrEmployee(_iBookingRepository.Read(id).Accommodation.Owner.Id, _iBookingRepository.Read(id).BookedBy.Id))
            {
                Booking booking = IBookingRepository.Read(id);

                if (booking == null)
                    throw new IdNotFoundException("Booking", id);

                return booking;
            }
            throw new AccessException();
        }

        public List<Booking> GetBookingsOfOwnedAccommodation(int userid)
        {
            AnyoneLoggedIn();

            List<Booking> bookingsOfOwnedAccommodation = new List<Booking>();

            FindAllBookingsOfOwnedAccommodation(userid, bookingsOfOwnedAccommodation);

            if (bookingsOfOwnedAccommodation.Count == 0)
                throw new NoneFoundInDatabaseTableException(userid, "bookings of owned accommodations");

            return bookingsOfOwnedAccommodation;
        }

        private void FindAllBookingsOfOwnedAccommodation(int userid, List<Booking> bookingsOfOwnedAccommodation)
        {
            foreach (var booking in IBookingRepository.GetAll())
            {
                if (booking.Accommodation.Owner.Id == userid)
                    bookingsOfOwnedAccommodation.Add(booking);
            }
        }

        public List<Booking> GetAllBookings()
        {
            if (HasElevatedRights())
            {
                return GetAllInSystem();
            }
            else
            {
                return GetOnlyOnesOwnedByUser();
            }
        }

        private List<Booking> GetAllInSystem()
        {
            List<Booking> all = IBookingRepository.GetAll();

            if (all.Count == 0)
                throw new NoneFoundInDatabaseTableException("bookings");

            return all;
        }

        private List<Booking> GetOnlyOnesOwnedByUser()
        {
            List<Booking> bookingsOfLoggedInUser = new List<Booking>();

            FindAllBookingsOfLoggedInUser(bookingsOfLoggedInUser);

            if (bookingsOfLoggedInUser.Count == 0)
                throw new NoneFoundInDatabaseTableException(LoggedInAs.Id, "bookings");

            return bookingsOfLoggedInUser;
        }

        private void FindAllBookingsOfLoggedInUser(List<Booking> bookingsOfLoggedInUser)
        {
            foreach (var booking in IBookingRepository.GetAll())
            {
                if (booking.BookedBy.Id == LoggedInAs.Id)
                {
                    bookingsOfLoggedInUser.Add(booking);
                }
            }
        }

        public Booking Book(string startDate, User booker, int nights, Accommodation accommodation)
        {
            if (booker.Id == accommodation.Owner.Id)
                throw new ParameterException("Accommodation", "be booked by the owner");

            Booking booking = BookIfAvailableAndUserHasPermission(startDate, booker, nights, accommodation);
            IBookingRepository.Create(booking);
            return booking;
        }

        private Booking BookIfAvailableAndUserHasPermission(string startDate, User booker, int nights, Accommodation accommodation)
        {
            if (CheckIfUserIsAllowedToPerformAction(booker.Id))
            {
                if (nights < 1)
                    throw new ParameterException("Nights", "less than one");

                startDate = startDate.Trim();
                return TryToBookIfAllDatesAvailable(startDate, booker, nights, accommodation);
            }
            throw new AccessException($"Must be performed by a customer with ID {booker.Id}, or by admin or an employee on behalf of a customer with ID {booker.Id}!");
        }

        private Booking TryToBookIfAllDatesAvailable(string startDate, User booker, int nights, Accommodation accommodation)
        {
            if (AreAllDatesAvailable(accommodation.Schedule, startDate, nights))
            {
                List<string> datesBooked = new List<string>();
                AddDatesToList(datesBooked, startDate, nights);

                SetStatusToUnavailable(accommodation, datesBooked);

                int totalPrice = nights * accommodation.PricePerNight;
                Booking booking = new Booking(booker, accommodation, datesBooked, totalPrice);
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

            UpdateScheduleInDb(accommodation.Id, accommodation.Schedule);
        }

        public void UpdateBooking(string newStartDate, int nights, int bookingId)
        {
            Booking originalBooking = IBookingRepository.Read(bookingId);
            int deadlineInDays = originalBooking.Accommodation.CancellationDeadlineInDays;

            if (CancelationDeadlineCheck(originalBooking.Dates[0], deadlineInDays))
            {
                Booking newBooking = BookIfAvailableAndUserHasPermission(newStartDate, originalBooking.BookedBy, nights, originalBooking.Accommodation);
                UpdateTheDatesOfTheScheduleInTheDb(originalBooking, originalBooking.Dates, originalBooking.Accommodation.Schedule);
                IBookingRepository.Update(bookingId, newBooking);
            }
            else
                throw new CancelBookingException(originalBooking.Id, deadlineInDays);
        }
    }
}
