using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Core.Models;
using AintBnB.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AintBnB.BusinessLogic.Helpers.DateHelper;

namespace AintBnB.BusinessLogic.Imp
{
    public class DeletionService : IDeletionService
    {
        private IUnitOfWork _unitOfWork;

        public DeletionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>Delete a user.</summary>
        /// <param name="id">The user-ID of the user to delete.</param>
        public async Task DeleteUserAsync(int id)
        {
            var user = await _unitOfWork.UserRepository.ReadAsync(id);
            CheckIfUserCanBeDeleted(user);
            await DeleteUsersAccommodationsAsync(id);
            await DeleteUsersBookingsAsync(id);
            await _unitOfWork.UserRepository.DeleteAsync(id);
            await _unitOfWork.CommitAsync();
        }

        /// <summary>Checks if the user can be deleted.</summary>
        /// <param name="user">The user that will be deleted.</param>
        /// <exception cref="NotFoundException">User is not found in the database</exception>
        /// <exception cref="AccessException">If the user to be deleted is an admin because admin can't be deleted!</exception>
        private void CheckIfUserCanBeDeleted(User user)
        {
            if (user == null)
                throw new NotFoundException("User", user.Id);

            if (user.UserType == UserTypes.Admin)
                throw new AccessException("Admin cannot be deleted!");
        }

        /// <summary>Deletes the accommodations of a user to be deleted.</summary>
        /// <param name="id">The user-ID of the user to be deleted.</param>
        /// <exception cref="CancelBookingException">If an accommodation owned by the user can't be deleted because it has bookings that can't be canceled if the cancellation deadline has expired</exception>
        private async Task DeleteUsersAccommodationsAsync(int id)
        {
            var accommodationsToBeDeleted = new List<Accommodation>();

            foreach (var accommodation in await _unitOfWork.AccommodationRepository.GetAllAsync())
            {
                if (accommodation.Owner.Id == id)
                {
                    try
                    {
                        await CanAccommodationBookingsBeDeletedAsync(accommodation.Id);
                    }
                    catch (Exception)
                    {
                        throw new CancelBookingException(new int[] { id, accommodation.Id, accommodation.CancellationDeadlineInDays });
                    }

                    accommodationsToBeDeleted.Add(accommodation);
                }
            }

            foreach (var acc in accommodationsToBeDeleted)
            {
                await _unitOfWork.AccommodationRepository.DeleteAsync(acc.Id);
                DeleteAllPicturesOfAccommodation(acc.Id);
            }
        }

        /// <summary>Deletes an image from the list of images of an accommodation.</summary>
        /// <param name="imageId">The Id of the image to delete.</param>
        public async Task DeletePictureAsync(int imageId)
        {
            var img = await _unitOfWork.ImageRepository.ReadAsync(imageId);

            if (img == null)
                throw new NotFoundException("Image", imageId);

            _unitOfWork.ImageRepository.Delete(img);

            await _unitOfWork.CommitAsync();
        }

        private void DeleteAllPicturesOfAccommodation(int accommodationId)
        {
            foreach (var pic in _unitOfWork.ImageRepository.GetAll(accommodationId))
                _unitOfWork.ImageRepository.Delete(pic);
        }

        /// <summary>Deletes the bookings of the user to be deleted.</summary>
        /// <param name="id">The user-ID of the user to be deleted.</param>
        /// <exception cref="CancelBookingException">If a booking can't be deleted because the cancellation deadline has expired</exception>
        private async Task DeleteUsersBookingsAsync(int id)
        {
            var bookingsToBeDeleted = new List<Booking>();

            foreach (var booking in await _unitOfWork.BookingRepository.GetAllAsync())
            {
                if (booking.BookedBy.Id == id)
                {
                    try
                    {
                        await DeadLineExpirationAsync(booking.Id, booking.Accommodation.CancellationDeadlineInDays);
                    }
                    catch (Exception)
                    {
                        throw new CancelBookingException("user", booking.Id, booking.Accommodation.CancellationDeadlineInDays);
                    }

                    bookingsToBeDeleted.Add(booking);
                }
            }

            foreach (var booking in bookingsToBeDeleted)
                await DeleteTheBookingAsync(booking.Id);
        }

        /// <summary>Deletes an accommodation.</summary>
        /// <param name="id">The ID of the accommodation to be deleted.</param>
        public async Task DeleteAccommodationAsync(int id)
        {
            var accommodation = await _unitOfWork.AccommodationRepository.ReadAsync(id);

            if (accommodation == null)
                throw new NotFoundException("Accommodation", accommodation.Id);

            await CanAccommodationBookingsBeDeletedAsync(accommodation.Id);

            await _unitOfWork.AccommodationRepository.DeleteAsync(id);
            DeleteAllPicturesOfAccommodation(id);
            await _unitOfWork.CommitAsync();
        }

        /// <summary>Determines whether the bookings of the accommodation to be deleted can be cancelled.</summary>
        /// <param name="accommodationId">The ID of the accommodation to be deleted.</param>
        /// <exception cref="CancelBookingException">The accommodation can't be deleted because it has bookings that can't be cancelled because the cancellation deadline has expired</exception>
        private async Task CanAccommodationBookingsBeDeletedAsync(int accommodationId)
        {
            var toDelete = new List<Booking>();

            foreach (var booking in await _unitOfWork.BookingRepository.GetAllAsync())
            {
                if (booking.Accommodation.Id == accommodationId)
                {
                    try
                    {
                        await DeadLineExpirationAsync(booking.Id, booking.Accommodation.CancellationDeadlineInDays);
                    }
                    catch (Exception)
                    {
                        throw new CancelBookingException($"accommodation with ID {booking.Accommodation.Id}", booking.Id, booking.Accommodation.CancellationDeadlineInDays);
                    }

                    toDelete.Add(booking);
                }
            }

            foreach (var booking in toDelete)
                await DeleteTheBookingAsync(booking.Id);
        }

        /// <summary>Deletes a booking.</summary>
        /// <param name="id">The ID of the booking to be deleted.</param>
        /// <exception cref="NotFoundException">Booking not found in the database</exception>
        public async Task DeleteBookingAsync(int id)
        {
            var booking = await _unitOfWork.BookingRepository.ReadAsync(id);

            if (booking == null)
                throw new NotFoundException("Booking", id);

            await DeadLineExpirationAsync(id, booking.Accommodation.CancellationDeadlineInDays);

            await DeleteTheBookingAsync(id);

            await _unitOfWork.CommitAsync();
        }


        /// <summary>Checks if the cancellation deadline has expired.</summary>
        /// <param name="id">The ID of the booking to be deleted.</param>
        /// <param name="deadlineInDays">The cancellation deadline in days.</param>
        /// <exception cref="CancelBookingException">If the booking can't be deleted because the cancellation deadline has expired</exception>
        private async Task DeadLineExpirationAsync(int id, int deadlineInDays)
        {
            var booking = await _unitOfWork.BookingRepository.ReadAsync(id);

            if (DateIsInThePast(booking.Dates[booking.Dates.Count - 1]))
                return;

            var firstDateBooked = booking.Dates[0];

            if (!CancelationDeadlineCheck(firstDateBooked, deadlineInDays))
                throw new CancelBookingException(id, deadlineInDays);
        }

        /// <summary>Deletes the booking from the database.</summary>
        /// <param name="id">The ID of the booking to delete.</param>
        private async Task DeleteTheBookingAsync(int id)
        {
            await ResetAvailableStatusAfterDeletingBookingAsync(id);
            await _unitOfWork.BookingRepository.DeleteAsync(id);
        }

        /// <summary>Sets the status of the cancelled dates to available in the accommodation's schedule.</summary>
        /// <param name="id">The ID of the booking that was cancelled.</param>
        private async Task ResetAvailableStatusAfterDeletingBookingAsync(int id)
        {
            var booking = await _unitOfWork.BookingRepository.ReadAsync(id);

            if (DateIsInThePast(booking.Dates[booking.Dates.Count - 1]))
                return;

            ResetDatesToAvailable(booking.Dates, booking.Accommodation.Schedule);
            await _unitOfWork.AccommodationRepository.UpdateAsync(booking.Accommodation.Id, booking.Accommodation);
        }
    }
}