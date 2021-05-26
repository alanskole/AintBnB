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

namespace AintBnB.BusinessLogic.Imp
{
    public class BookingService : IBookingService
    {
        private IUnitOfWork _unitOfWork;

        public BookingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>Books an accommodation.</summary>
        /// <param name="startDate">The start date of the booking.</param>
        /// <param name="booker">The booker.</param>
        /// <param name="nights">The amount of nights to book for.</param>
        /// <param name="accommodation">The accommodation to book.</param>
        /// <returns>The booking object</returns>
        /// <exception cref="ParameterException">Accommodation can't be booked by its owner
        /// or
        /// Nights can't be less than one</exception>
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

        /// <summary>Books if the dates are available and user is allowed to make the booking.</summary>
        /// <param name="startDate">The start date of the booking.</param>
        /// <param name="booker">The booker.</param>
        /// <param name="nights">The amount of nights to book for.</param>
        /// <param name="accommodation">The accommodation to book.</param>
        /// <returns>The booking object</returns>
        /// <exception cref="AccessException">If the user that wants to book isn't the booker or admin booking on behalf of the booker</exception>
        private Booking BookIfAvailableAndUserHasPermission(string startDate, User booker, int nights, Accommodation accommodation)
        {
            if (CheckIfUserIsAllowedToPerformAction(booker))
            {
                startDate = startDate.Trim();
                return TryToBookIfAllDatesAvailable(startDate, booker, nights, accommodation);
            }
            throw new AccessException($"Must be performed by a customer with ID {booker.Id}, or by admin on behalf of a customer with ID {booker.Id}!");
        }

        /// <summary>Makes the booking of all the dates are avaiable.</summary>
        /// <param name="startDate">The start date of the booking.</param>
        /// <param name="booker">The booker.</param>
        /// <param name="nights">The amount of nights to book for.</param>
        /// <param name="accommodation">The accommodation to book.</param>
        /// <returns>The booking object</returns>
        /// <exception cref="DateException">If all the dates aren't available</exception>
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

        /// <summary>Adds the booked dates to the list dates the booking consists of.</summary>
        /// <param name="datesBooked">A list that will be filled with all the dates of the booking.</param>
        /// <param name="startDate">The start date of the booking.</param>
        /// <param name="nights">The amount of nights to book for.</param>
        private void AddDatesToList(List<string> datesBooked, string startDate, int nights)
        {
            for (int i = 0; i < nights; i++)
            {
                string dateToAdd = DateFormatterCustomDate(DateTime.Parse(startDate).AddDays(i));
                datesBooked.Add(dateToAdd);
            }
        }

        /// <summary>Marks the booked dates as unavaiable in the schedule.</summary>
        /// <param name="accommodation">The accommodation that was booked.</param>
        /// <param name="datesBooked">The list with the dates that were booked and must be marked as unavailable.</param>
        private void SetStatusToUnavailable(Accommodation accommodation, List<string> datesBooked)
        {
            for (int i = 0; i < datesBooked.Count; i++)
                accommodation.Schedule[datesBooked[i]] = false;
        }

        /// <summary>Updates the dates of an existing booking.</summary>
        /// <param name="newStartDate">The new start date.</param>
        /// <param name="nights">The amount of nights to book.</param>
        /// <param name="bookingId">The booking-ID of the booking to update.</param>
        /// <exception cref="DateException">Can't update a booking that has a checkout date that's in the past!
        /// or
        /// Not all dates new are available, can't update the booking dates!</exception>
        public async Task UpdateBookingAsync(string newStartDate, int nights, int bookingId)
        {
            var originalBooking = await _unitOfWork.BookingRepository.ReadAsync(bookingId);

            if (DateIsInThePast(originalBooking.Dates[originalBooking.Dates.Count - 1]))
                throw new DateException("Can't update a booking that has a checkout date that's in the past!");

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

        /// <summary>Determines whether the booking can be updated.</summary>
        /// <param name="newStartDate">The new start date.</param>
        /// <param name="nights">The amount of nights to book.</param>
        /// <param name="originalBooking">The original booking that needs to be updated.</param>
        /// <exception cref="AccessException">Must be performed by the booker, or by admin on behalf of the booker!</exception>
        /// <exception cref="ParameterException">Nights can't be less than one
        /// or
        /// Updated dates can't the same as original dates</exception>
        /// <exception cref="CancelBookingException"></exception>
        private static void CanBookingBeUpdated(string newStartDate, int nights, Booking originalBooking)
        {
            if (!CheckIfUserIsAllowedToPerformAction(originalBooking.BookedBy))
                throw new AccessException($"Must be performed by the booker, or by admin on behalf of the booker!");

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

        /// <summary>Updateds the dates of the booking.</summary>
        /// <param name="newStartDate">The new start date.</param>
        /// <param name="nights">The amount of nights to book.</param>
        /// <param name="newDates">The set of the new dates.</param>
        private static void UpdatedDates(string newStartDate, int nights, SortedSet<string> newDates)
        {
            for (int i = 0; i < nights; i++)
            {
                var date = DateFormatterCustomDate(DateTime.Parse(newStartDate).AddDays(i));
                newDates.Add(date);
            }
        }

        /// <summary>Dates from original booking that needs to be canceled.</summary>
        /// <param name="newStartDate">The new start date.</param>
        /// <param name="datesOriginal">The set of original dates that must be canceled.</param>
        /// <param name="newDates">The new dates to be booked.</param>
        /// <param name="originalBookingStartDate">The original booking start date.</param>
        /// <param name="originalBookingEndDate">The original checkout date of the booking.</param>
        /// <param name="newBookingStartDate">The updated booking start date.</param>
        /// <param name="newBookingEndDate">The updated booking checkout date.</param>
        /// <param name="datesToRemove">The set of dates that needs to be canceled and marked as available in the schedule of the accommodation.</param>
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

        /// <summary>If the updated dates are within the range of the original start and checkout dates of the booking.</summary>
        /// <param name="datesOriginal">The set of the original booking dates.</param>
        /// <param name="newDates">The set of the updates dates.</param>
        /// <param name="datesToRemove">The set of the dates to cancel.</param>
        private static void NewDatesAreBetweenOriginalDates(SortedSet<string> datesOriginal, SortedSet<string> newDates, SortedSet<string> datesToRemove)
        {
            foreach (var date in datesOriginal)
            {
                if (!newDates.Contains(date))
                    datesToRemove.Add(date);
            }
        }

        /// <summary>If the start date of the updated dates is within the range of the original start and checkout date of the booking, but the new checkout date is after the original checkout date.</summary>
        /// <param name="newStartDate">The new start date.</param>
        /// <param name="datesOriginal">The set of the original booking dates.</param>
        /// <param name="datesToRemove">The set of the dates to cancel.</param>
        private static void OnlyStartOfNewDatesAreBetweenOldDates(string newStartDate, SortedSet<string> datesOriginal, SortedSet<string> datesToRemove)
        {
            foreach (var date in datesOriginal)
            {
                if (date == newStartDate)
                    break;

                datesToRemove.Add(date);
            }
        }

        /// <summary>If the start date of the updated dates is before the original start but the new checkout date between the orignal start and checkout date.</summary>
        /// <param name="datesOriginal">The set of the original booking dates.</param>
        /// <param name="newDates">The set of the new dates.</param>
        /// <param name="datesToRemove">The set of the dates to cancel.</param>
        private static void OnlyEndDateIsBetweenOldDates(SortedSet<string> datesOriginal, SortedSet<string> newDates, SortedSet<string> datesToRemove)
        {
            foreach (var date in datesOriginal)
            {
                if (!newDates.Contains(date))
                    datesToRemove.Add(date);
            }
        }

        /// <summary>Fetches a booking.</summary>
        /// <param name="id">The ID of the booking to get.</param>
        /// <returns>The booking object</returns>
        /// <exception cref="IdNotFoundException">No bookings found with the booking-ID</exception>
        /// <exception cref="AccessException">The user wants to get the booking isn't the booker, the owner of the accommodation that was booked or admin</exception>
        public async Task<Booking> GetBookingAsync(int id)
        {
            var booking = await _unitOfWork.BookingRepository.ReadAsync(id);

            if (booking == null)
                throw new IdNotFoundException("Booking", id);

            if (CorrectUserOrOwnerOrAdmin(booking.Accommodation.Owner.Id, booking.BookedBy))
                return booking;

            throw new AccessException();
        }

        /// <summary>Gets all the bookings of a users accommodations.</summary>
        /// <param name="userid">The user-ID of the owner of the accommoations.</param>
        /// <returns>A list of all the booking objects</returns>
        /// <exception cref="NoneFoundInDatabaseTableException">No bookings found</exception>
        /// <exception cref="AccessException">The user that calls this method isn't the owner of the accommodations or admin</exception>
        public async Task<List<Booking>> GetBookingsOfOwnedAccommodationAsync(int userid)
        {
            var user = await _unitOfWork.UserRepository.ReadAsync(userid);
            if (CorrectUserOrAdmin(user.Id))
            {
                var bookingsOfOwnedAccommodation = new List<Booking>();

                await FindAllBookingsOfOwnedAccommodationAsync(userid, bookingsOfOwnedAccommodation);

                if (bookingsOfOwnedAccommodation.Count == 0)
                    throw new NoneFoundInDatabaseTableException(userid, "bookings of owned accommodations");

                return bookingsOfOwnedAccommodation;
            }
            throw new AccessException();
        }

        /// <summary>Finds all bookings made of the accommodations of a user.</summary>
        /// <param name="userid">The user-ID of the owner of the accommodations.</param>
        /// <param name="bookingsOfOwnedAccommodation">The list to put the bookings that were found in.</param>
        private async Task FindAllBookingsOfOwnedAccommodationAsync(int userid, List<Booking> bookingsOfOwnedAccommodation)
        {
            foreach (var booking in await _unitOfWork.BookingRepository.GetAllAsync())
            {
                if (booking.Accommodation.Owner.Id == userid)
                    bookingsOfOwnedAccommodation.Add(booking);
            }
        }

        /// <summary>Gets all bookings in the database.</summary>
        /// <returns>A list of all the bookings if method is called by admin. If the method is called by a customer it fetches all the bookings of the customer</returns>
        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            if (AdminChecker())
            {
                return await GetAllInSystemAsync();
            }
            else
            {
                return await GetOnlyOnesOwnedByUserAsync();
            }
        }

        /// <summary>Gets all the bookings in the database.</summary>
        /// <returns>A list of all the bookings</returns>
        /// <exception cref="NoneFoundInDatabaseTableException">No bookings found in the database</exception>
        private async Task<List<Booking>> GetAllInSystemAsync()
        {
            var all = await _unitOfWork.BookingRepository.GetAllAsync();

            if (all.Count == 0)
                throw new NoneFoundInDatabaseTableException("bookings");

            return all;
        }

        /// <summary>Gets all the bookings of a user.</summary>
        /// <returns>A list of all the bookings of the user</returns>
        /// <exception cref="NoneFoundInDatabaseTableException">No bookings belonging to the user found in the database</exception>
        private async Task<List<Booking>> GetOnlyOnesOwnedByUserAsync()
        {
            var bookingsOfLoggedInUser = new List<Booking>();

            await FindAllBookingsOfLoggedInUserAsync(bookingsOfLoggedInUser);

            if (bookingsOfLoggedInUser.Count == 0)
                throw new NoneFoundInDatabaseTableException(LoggedInAs.Id, "bookings");

            return bookingsOfLoggedInUser;
        }

        /// <summary>Finds all bookings of the user.</summary>
        /// <param name="bookingsOfLoggedInUser">The list to put the bookings of the user in.</param>
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

        /// <summary>Rates the accommodation that was booked after a booking has ended.</summary>
        /// <param name="bookingId">The booking-ID of the booking to leave the rating for.</param>
        /// <param name="rating">A rating between 1-5.</param>
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

        /// <summary>Determines whether a rating can be given.</summary>
        /// <param name="booking">The booking to leave the rating for.</param>
        /// <param name="booker">The booker.</param>
        /// <param name="rating">The rating between 1-5.</param>
        /// <exception cref="AccessException">If any other user than the booker tries to rate</exception>
        /// <exception cref="ParameterException">Rating can't be less than 1 or bigger than 5
        /// or
        /// Rating can't be given twice
        /// or
        /// Rating can't be given until after checking out</exception>
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
