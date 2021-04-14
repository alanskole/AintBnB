using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Core.Models;
using AintBnB.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AintBnB.BusinessLogic.Helpers.Authentication;
using static AintBnB.BusinessLogic.Helpers.DateHelper;
using static AintBnB.BusinessLogic.Helpers.UpdateCancelledDatesInSchedule;

namespace AintBnB.BusinessLogic.Imp
{
    public class BookingService : IBookingService
    {
        private IUnitOfWork _unitOfWork;

        public BookingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Booking> BookAsync(string startDate, User booker, int nights, Accommodation accommodation)
        {
            if (booker.Id == accommodation.Owner.Id)
                throw new ParameterException("Accommodation", "booked by the owner");

            if (nights < 1)
                throw new ParameterException("Nights", "less than one");

            var booking = BookIfAvailableAndUserHasPermission(startDate, booker, nights, accommodation);

            await _unitOfWork.BookingRepository.CreateAsync(booking);

            await _unitOfWork.AccommodationRepository.UpdateAsync(accommodation.Id, accommodation);

            await _unitOfWork.CommitAsync();

            return booking;
        }

        private Booking BookIfAvailableAndUserHasPermission(string startDate, User booker, int nights, Accommodation accommodation)
        {
            if (CheckIfUserIsAllowedToPerformAction(booker))
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
                var datesBooked = new List<string>();
                AddDatesToList(datesBooked, startDate, nights);

                SetStatusToUnavailable(accommodation, datesBooked);

                var totalPrice = nights * accommodation.PricePerNight;
                var booking = new Booking(booker, accommodation, datesBooked, totalPrice);
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
        }

        public async Task UpdateBookingAsync(string newStartDate, int nights, int bookingId)
        {
            var originalBooking = await _unitOfWork.BookingRepository.ReadAsync(bookingId);

            CanBookingBeUpdated(newStartDate, nights, originalBooking);

            if (!AreAllDatesAvailable(originalBooking.Accommodation.Schedule, newStartDate, nights))
            {
                var unavailableDates = new SortedSet<string>(GetUnavailableDates(originalBooking.Accommodation.Schedule, newStartDate, nights));

                var datesOriginal = new SortedSet<string>(originalBooking.Dates);

                if (!datesOriginal.Contains(unavailableDates.Min) || !datesOriginal.Contains(unavailableDates.Max))
                    throw new DateException("Not all dates are available, cannot update the booking dates!");

                var newDates = new SortedSet<string>();

                UpdatedDates(newStartDate, nights, newDates);

                var originalBookingStartDate = DateTime.Parse(datesOriginal.Min);
                var originalBookingEndDate = DateTime.Parse(datesOriginal.Max);
                var newBookingStartDate = DateTime.Parse(newStartDate);
                var newBookingEndDate = DateTime.Parse(newStartDate).AddDays(nights);

                var datesToRemove = new SortedSet<string>();

                DatesFromOriginalBookingToCancel(newStartDate, datesOriginal, newDates, originalBookingStartDate, originalBookingEndDate, newBookingStartDate, newBookingEndDate, datesToRemove);

                datesOriginal.ExceptWith(datesToRemove);
                datesOriginal.UnionWith(newDates);

                ResetDatesToAvailable(datesToRemove.ToList(), originalBooking.Accommodation.Schedule);

                originalBooking.Dates = datesOriginal.ToList();

                originalBooking.Price = originalBooking.Dates.Count * originalBooking.Accommodation.PricePerNight;

                SetStatusToUnavailable(originalBooking.Accommodation, originalBooking.Dates);
            }
            else
            {
                var oldDates = originalBooking.Dates;
                originalBooking = BookIfAvailableAndUserHasPermission(newStartDate, originalBooking.BookedBy, nights, originalBooking.Accommodation);
                ResetDatesToAvailable(oldDates, originalBooking.Accommodation.Schedule);
            }
            await _unitOfWork.BookingRepository.UpdateAsync(bookingId, originalBooking);

            await _unitOfWork.AccommodationRepository.UpdateAsync(originalBooking.Accommodation.Id, originalBooking.Accommodation);

            await _unitOfWork.CommitAsync();
        }

        private static void CanBookingBeUpdated(string newStartDate, int nights, Booking originalBooking)
        {
            if (!CheckIfUserIsAllowedToPerformAction(originalBooking.BookedBy))
                throw new AccessException($"Must be performed by the booker, or by admin or an employee on behalf of the booker!");

            if (nights < 1)
                throw new ParameterException("Nights", "less than one");

            if (originalBooking.Dates[0] == newStartDate && originalBooking.Dates.Count == nights)
                throw new ParameterException("Updated dates", "the same as original dates");

            if (originalBooking.Dates[0] != newStartDate)
            {
                var deadlineInDays = originalBooking.Accommodation.CancellationDeadlineInDays;

                if (!CancelationDeadlineCheck(originalBooking.Dates[0], deadlineInDays))
                    throw new CancelBookingException(originalBooking.Id, deadlineInDays);
            }
        }

        private static void UpdatedDates(string newStartDate, int nights, SortedSet<string> newDates)
        {
            for (int i = 0; i < nights; i++)
            {
                var date = DateFormatterCustomDate(DateTime.Parse(newStartDate).AddDays(i));
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

        public async Task<Booking> GetBookingAsync(int id)
        {
            var booking = await _unitOfWork.BookingRepository.ReadAsync(id);

            if (booking == null)
                throw new IdNotFoundException("Booking", id);

            if (CorrectUserOrOwnerOrAdminOrEmployee(booking.Accommodation.Owner.Id, booking.BookedBy))
                return booking;
            
            throw new AccessException();
        }

        public async Task<List<Booking>> GetBookingsOfOwnedAccommodationAsync(int userid)
        {
            if (CorrectUserOrAdminOrEmployee(await _unitOfWork.UserRepository.ReadAsync(userid)))
            {
                var bookingsOfOwnedAccommodation = new List<Booking>();

                await FindAllBookingsOfOwnedAccommodationAsync(userid, bookingsOfOwnedAccommodation);

                if (bookingsOfOwnedAccommodation.Count == 0)
                    throw new NoneFoundInDatabaseTableException(userid, "bookings of owned accommodations");

                return bookingsOfOwnedAccommodation;
            }
            throw new AccessException();
        }

        private async Task FindAllBookingsOfOwnedAccommodationAsync(int userid, List<Booking> bookingsOfOwnedAccommodation)
        {
            foreach (var booking in await _unitOfWork.BookingRepository.GetAllAsync())
            {
                if (booking.Accommodation.Owner.Id == userid)
                    bookingsOfOwnedAccommodation.Add(booking);
            }
        }

        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            if (HasElevatedRights())
            {
                return await GetAllInSystemAsync();
            }
            else
            {
                return await GetOnlyOnesOwnedByUserAsync();
            }
        }

        private async Task<List<Booking>> GetAllInSystemAsync()
        {
            var all = await _unitOfWork.BookingRepository.GetAllAsync();

            if (all.Count == 0)
                throw new NoneFoundInDatabaseTableException("bookings");

            return all;
        }

        private async Task<List<Booking>> GetOnlyOnesOwnedByUserAsync()
        {
            var bookingsOfLoggedInUser = new List<Booking>();

            await FindAllBookingsOfLoggedInUserAsync(bookingsOfLoggedInUser);

            if (bookingsOfLoggedInUser.Count == 0)
                throw new NoneFoundInDatabaseTableException(LoggedInAs.Id, "bookings");

            return bookingsOfLoggedInUser;
        }

        private async Task FindAllBookingsOfLoggedInUserAsync(List<Booking> bookingsOfLoggedInUser)
        {
            foreach (var booking in await _unitOfWork.BookingRepository.GetAllAsync())
            {
                if (booking.BookedBy.Id == LoggedInAs.Id)
                {
                    bookingsOfLoggedInUser.Add(booking);
                }
            }
        }

        public async Task RateAsync(int bookingId, int rating)
        {
            AnyoneLoggedIn();

            var booking = await GetBookingAsync(bookingId);

            CanRatingBeGiven(booking, booking.BookedBy, rating);

            var accommodation = booking.Accommodation;

            booking.Rating = rating;

            var currentRating = accommodation.AverageRating;
            var amountOfRatings = accommodation.AmountOfRatings;

            accommodation.AverageRating = (currentRating + rating) / (amountOfRatings + 1);
            accommodation.AmountOfRatings += 1;

            await _unitOfWork.CommitAsync();
        }

        private void CanRatingBeGiven(Booking booking, User booker, int rating)
        {
            if (booker.Id != LoggedInAs.Id)
                throw new AccessException("Only the booker can leave a rating!");

            if (rating < 1 || rating > 5)
                throw new ParameterException("Rating", "less than 1 or bigger than 5");

            if (booking.Rating > 0)
                throw new ParameterException("Rating", "given twice");

            if (DateTime.Today <= DateTime.Parse(booking.Dates[booking.Dates.Count - 1]))
                throw new ParameterException("Rating", "given until after checking out");
        }
    }
}
