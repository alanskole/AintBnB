using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Core.Models;
using AintBnB.Repository.Interfaces;
using System;
using System.Threading.Tasks;
using static AintBnB.BusinessLogic.Helpers.Authentication;
using static AintBnB.BusinessLogic.Helpers.DateHelper;
using static AintBnB.BusinessLogic.Helpers.UpdateCancelledDatesInSchedule;

namespace AintBnB.BusinessLogic.Imp
{
    public class DeletionService : IDeletionService
    {
        private IUnitOfWork _unitOfWork;

        public DeletionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _unitOfWork.UserRepository.ReadAsync(id);
            if (CorrectUserOrAdmin(id))
            {
                CheckIfUserCanBeDeleted(user);
                await DeleteUsersAccommodationsAsync(id);
                await DeleteUsersBookingsAsync(id);
                await _unitOfWork.UserRepository.DeleteAsync(id);
                await _unitOfWork.CommitAsync();

                if (!AdminChecker())
                    Logout();
            }
            else if (user.UserType == UserTypes.Employee)
                throw new AccessException("Employees cannot delete any accounts, even if it's their own accounts!");
            else
                throw new AccessException($"Administrator or user with ID {id} only!");
        }

        private void CheckIfUserCanBeDeleted(User user)
        {
            if (user == null)
                throw new IdNotFoundException("User", user.Id);

            if (user.UserType == UserTypes.Admin)
                throw new AccessException("Admin cannot be deleted!");
        }

        private async Task DeleteUsersAccommodationsAsync(int id)
        {
            foreach (var accommodation in await _unitOfWork.AccommodationRepository.GetAllAsync())
            {
                if (accommodation.Owner == await _unitOfWork.UserRepository.ReadAsync(id))
                {
                    await DeleteAccommodationAsync(accommodation.Id);
                }
            }
        }

        private async Task DeleteUsersBookingsAsync(int id)
        {
            foreach (var booking in await _unitOfWork.BookingRepository.GetAllAsync())
            {
                if (booking.BookedBy == await _unitOfWork.UserRepository.ReadAsync(id))
                {
                    try
                    {
                        await DeleteBookingAsync(booking.Id);
                    }
                    catch (Exception)
                    {
                        throw new CancelBookingException("user", booking.Id, booking.Accommodation.CancellationDeadlineInDays);
                    }
                }
            }
        }

        public async Task DeleteAccommodationAsync(int id)
        {
            var accommodation = await _unitOfWork.AccommodationRepository.ReadAsync(id);

            CanAccommodationBeDeleted(accommodation);

            await DeleteAccommodationBookingsAsync(accommodation.Id);

            await _unitOfWork.AccommodationRepository.DeleteAsync(id);
            await _unitOfWork.CommitAsync();
        }

        private void CanAccommodationBeDeleted(Accommodation accommodation)
        {
            if (accommodation == null)
                throw new IdNotFoundException("Accommodation", accommodation.Id);

            if (!CorrectUserOrAdminOrEmployee(accommodation.Owner))
                throw new AccessException($"Administrator, employee or user with ID {accommodation.Owner.Id} only!");
        }

        private async Task DeleteAccommodationBookingsAsync(int accommodationId)
        {
            foreach (var booking in await _unitOfWork.BookingRepository.GetAllAsync())
            {
                if (booking.Accommodation == await _unitOfWork.AccommodationRepository.ReadAsync(accommodationId))
                {
                    try
                    {
                        await DeleteBookingAsync(booking.Id);
                    }
                    catch (Exception)
                    {
                        throw new CancelBookingException("accommodation", booking.Id, booking.Accommodation.CancellationDeadlineInDays);
                    }
                }
            }
        }

        public async Task DeleteBookingAsync(int id)
        {
            var booking = await _unitOfWork.BookingRepository.ReadAsync(id);

            if (booking == null)
                throw new IdNotFoundException("Booking", id);

            if (CorrectUserOrOwnerOrAdminOrEmployee(booking.Accommodation.Owner.Id, booking.BookedBy))
                await DeadLineExpirationAsync(id, booking.Accommodation.CancellationDeadlineInDays);
            else
                throw new AccessException();
        }

        private async Task DeadLineExpirationAsync(int id, int deadlineInDays)
        {
            var booking = await _unitOfWork.BookingRepository.ReadAsync(id);

            if (DateIsInThePast(booking.Dates[booking.Dates.Count - 1]))
            {
                await DeleteTheBookingAsync(id);
                return;
            }

            var firstDateBooked = booking.Dates[0];

            if (CancelationDeadlineCheck(firstDateBooked, deadlineInDays))
            {
                await ResetAvailableStatusAfterDeletingBookingAsync(id);
                await DeleteTheBookingAsync(id);
            }
            else
                throw new CancelBookingException(id, deadlineInDays);
        }

        private async Task DeleteTheBookingAsync(int id)
        {
            await _unitOfWork.BookingRepository.DeleteAsync(id);
            await _unitOfWork.CommitAsync();
        }

        private async Task ResetAvailableStatusAfterDeletingBookingAsync(int id)
        {
            var booking = await _unitOfWork.BookingRepository.ReadAsync(id);
            ResetDatesToAvailable(booking.Dates, booking.Accommodation.Schedule);
            await _unitOfWork.AccommodationRepository.UpdateAsync(booking.Accommodation.Id, booking.Accommodation);
        }
    }
}