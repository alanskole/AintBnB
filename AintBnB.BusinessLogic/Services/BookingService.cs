using AintBnB.Core.Models;
using AintBnB.BusinessLogic.DependencyProviderFactory;
using AintBnB.BusinessLogic.Repository;
using static AintBnB.BusinessLogic.Services.DateService;
using static AintBnB.BusinessLogic.Services.UpdateScheduleInDatabase;
using static AintBnB.BusinessLogic.Services.AuthenticationService;
using System;
using System.Collections.Generic;
using AintBnB.BusinessLogic.CustomExceptions;
using System.Linq;

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

            if (nights < 1)
                throw new ParameterException("Nights", "less than one");

            Booking booking = BookIfAvailableAndUserHasPermission(startDate, booker, nights, accommodation);
            IBookingRepository.Create(booking);
            return booking;
        }

        private Booking BookIfAvailableAndUserHasPermission(string startDate, User booker, int nights, Accommodation accommodation)
        {
            if (CheckIfUserIsAllowedToPerformAction(booker.Id))
            {

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
            
            CanBookingBeUpdated(newStartDate, nights, originalBooking);

            if (!AreAllDatesAvailable(originalBooking.Accommodation.Schedule, newStartDate, nights))
            {
                SortedSet<string> unavailableDates = new SortedSet<string>(GetUnavailableDates(originalBooking.Accommodation.Schedule, newStartDate, nights));

                SortedSet<string> datesOriginal = new SortedSet<string>(originalBooking.Dates);

                if (!datesOriginal.Contains(unavailableDates.Min) || !datesOriginal.Contains(unavailableDates.Max))
                    throw new DateException("Not all dates are available, cannot update the booking dates!");

                SortedSet<string> newDates = new SortedSet<string>();

                UpdatedDates(newStartDate, nights, newDates);

                DateTime originalBookingStartDate = DateTime.Parse(datesOriginal.Min);
                DateTime originalBookingEndDate = DateTime.Parse(datesOriginal.Max);
                DateTime newBookingStartDate = DateTime.Parse(newStartDate);
                DateTime newBookingEndDate = DateTime.Parse(newStartDate).AddDays(nights);

                SortedSet<string> datesToRemove = new SortedSet<string>();

                DatesFromOriginalBookingToCancel(newStartDate, datesOriginal, newDates, originalBookingStartDate, originalBookingEndDate, newBookingStartDate, newBookingEndDate, datesToRemove);

                datesOriginal.ExceptWith(datesToRemove);
                datesOriginal.UnionWith(newDates);

                ResetCancelledDatesToAvailable(originalBooking, datesToRemove);

                originalBooking.Dates = datesOriginal.ToList();

                originalBooking.Price = originalBooking.Dates.Count * originalBooking.Accommodation.PricePerNight;
            }
            else
            {
                originalBooking = BookIfAvailableAndUserHasPermission(newStartDate, originalBooking.BookedBy, nights, originalBooking.Accommodation);
                UpdateTheDatesOfTheScheduleInTheDb(originalBooking, originalBooking.Dates, originalBooking.Accommodation.Schedule);
            }
            IBookingRepository.Update(bookingId, originalBooking);
        }

        private static void CanBookingBeUpdated(string newStartDate, int nights, Booking originalBooking)
        {
            if (!CheckIfUserIsAllowedToPerformAction(originalBooking.BookedBy.Id))
                throw new AccessException($"Must be performed by the booker, or by admin or an employee on behalf of the booker!");

            if (nights < 1)
                throw new ParameterException("Nights", "less than one");

            if (originalBooking.Dates[0] == newStartDate && originalBooking.Dates.Count == nights)
                throw new ParameterException("Updated dates", "the same as original dates");

            if (originalBooking.Dates[0] != newStartDate)
            {
                int deadlineInDays = originalBooking.Accommodation.CancellationDeadlineInDays;

                if (!CancelationDeadlineCheck(originalBooking.Dates[0], deadlineInDays))
                    throw new CancelBookingException(originalBooking.Id, deadlineInDays);
            }
        }

        private static void UpdatedDates(string newStartDate, int nights, SortedSet<string> newDates)
        {
            for (int i = 0; i < nights; i++)
            {
                string date = DateFormatterCustomDate(DateTime.Parse(newStartDate).AddDays(i));
                newDates.Add(date);
            }
        }

        private static void DatesFromOriginalBookingToCancel(string newStartDate, SortedSet<string> datesOriginal, SortedSet<string> newDates, DateTime originalBookingStartDate, DateTime originalBookingEndDate, DateTime newBookingStartDate, DateTime newBookingEndDate, SortedSet<string> datesToRemove)
        {
            if (newBookingStartDate >= originalBookingStartDate && newBookingStartDate <= originalBookingEndDate
                                && newBookingEndDate >= originalBookingStartDate && newBookingEndDate <= originalBookingEndDate)
            {
                NewDatesAreBetweenOriginalDates(datesOriginal, newDates, datesToRemove);
            }
            else if (newBookingStartDate >= originalBookingStartDate && newBookingStartDate <= originalBookingEndDate
                && !(newBookingEndDate >= originalBookingStartDate && newBookingEndDate <= originalBookingEndDate))
            {
                OnlyStartOfNewDatesAreBetweenOldDates(newStartDate, datesOriginal, datesToRemove);
            }
            else if (!(newBookingStartDate >= originalBookingStartDate && newBookingStartDate <= originalBookingEndDate)
                && newBookingEndDate >= originalBookingStartDate && newBookingEndDate <= originalBookingEndDate)
            {
                OnlyEndDateIsBetweenOldDates(datesOriginal, newDates, datesToRemove);
            }
        }

        private static void NewDatesAreBetweenOriginalDates(SortedSet<string> datesOriginal, SortedSet<string> newDates, SortedSet<string> datesToRemove)
        {
            foreach (var date in datesOriginal)
            {
                if (!newDates.Contains(date))
                    datesToRemove.Add(date);
            }
        }

        private static void OnlyStartOfNewDatesAreBetweenOldDates(string newStartDate, SortedSet<string> datesOriginal, SortedSet<string> datesToRemove)
        {
            foreach (var date in datesOriginal)
            {
                if (date == newStartDate)
                    break;

                datesToRemove.Add(date);
            }
        }

        private static void OnlyEndDateIsBetweenOldDates(SortedSet<string> datesOriginal, SortedSet<string> newDates, SortedSet<string> datesToRemove)
        {
            foreach (var date in datesOriginal)
            {
                if (!newDates.Contains(date))
                    datesToRemove.Add(date);
            }
        }

        private static void ResetCancelledDatesToAvailable(Booking originalBooking, SortedSet<string> datesToRemove)
        {
            foreach (var date in datesToRemove)
            {
                if (originalBooking.Accommodation.Schedule.ContainsKey(date))
                {
                    originalBooking.Accommodation.Schedule[date] = true;
                }
            }

            UpdateScheduleInDb(originalBooking.Accommodation.Id, originalBooking.Accommodation.Schedule);
        }

        public void Rate(int bookingId, int rating)
        {
            Booking booking = IBookingRepository.Read(bookingId);

            CanRatingBeGiven(booking, booking.BookedBy, rating);

            Accommodation accommodation = booking.Accommodation;

            booking.Rating = rating;

            double currentRating = accommodation.AverageRating;
            int amountOfRatings = accommodation.AmountOfRatings;

            accommodation.AverageRating = (currentRating + rating) / (amountOfRatings + 1);
            accommodation.AmountOfRatings += 1;

            IBookingRepository.Update(booking.Id, booking);
        }

        private static void CanRatingBeGiven(Booking booking, User booker, int rating)
        {
            if (booking == null)
                throw new NoneFoundInDatabaseTableException("booking");

            if (booker.Id != LoggedInAs.Id)
                throw new AccessException("Only the booker can leave a rating!");

            if (DateTime.Today <= DateTime.Parse(booking.Dates[booking.Dates.Count - 1]))
                throw new ParameterException("Rating", "given until after checking out");

            if (rating < 1 || rating > 5)
                throw new ParameterException("Rating", "less than 1 or bigger than 5");

            if (booking.Rating > 0)
                throw new ParameterException("Rating", "given twice");
        }
    }
}
