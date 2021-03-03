using AintBnB.Core.Models;
using System;
using static AintBnB.BusinessLogic.Helpers.DateHelper;
using static AintBnB.BusinessLogic.Helpers.UpdateCancelledDatesInSchedule;
using static AintBnB.BusinessLogic.Helpers.Authentication;
using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.Repository.Interfaces;
using AintBnB.BusinessLogic.Interfaces;

namespace AintBnB.BusinessLogic.Imp
{
    public class DeletionService : IDeletionService
    {
        private IUnitOfWork _unitOfWork;

        public DeletionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void DeleteUser(int id)
        {
            if (CorrectUserOrAdmin(id))
            {
                CheckIfUserCanBeDeleted(_unitOfWork.UserRepository.Read(id));
                DeleteUsersAccommodations(id);
                DeleteUsersBookings(id);
                _unitOfWork.UserRepository.Delete(id);
                _unitOfWork.Commit();
            }
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

        private void DeleteUsersAccommodations(int id)
        {
            foreach (Accommodation accommodation in _unitOfWork.AccommodationRepository.GetAll())
            {
                if (accommodation.Owner == _unitOfWork.UserRepository.Read(id))
                {
                    DeleteAccommodation(accommodation.Id);
                }
            }
        }

        private void DeleteUsersBookings(int id)
        {
            foreach (Booking booking in _unitOfWork.BookingRepository.GetAll())
            {
                if (booking.BookedBy == _unitOfWork.UserRepository.Read(id))
                {
                    try
                    {
                        DeleteBooking(booking.Id);
                    }
                    catch (Exception)
                    {
                        throw new CancelBookingException("user", id, booking.Accommodation.CancellationDeadlineInDays);
                    }
                }
            }
        }

        public void DeleteAccommodation(int id)
        {
            Accommodation accommodation = _unitOfWork.AccommodationRepository.Read(id);

            CanAccommodationBeDeleted(accommodation);

            DeleteAccommodationBookings(accommodation.Id);

            _unitOfWork.AccommodationRepository.Delete(id);
            _unitOfWork.Commit();
        }

        private void CanAccommodationBeDeleted(Accommodation accommodation)
        {
            if (accommodation == null)
                throw new IdNotFoundException("Accommodation", accommodation.Id);

            if (!CorrectUserOrAdminOrEmployee(accommodation.Owner))
                throw new AccessException($"Administrator, employee or user with ID {accommodation.Owner.Id} only!");
        }

        private void DeleteAccommodationBookings(int id)
        {
            foreach (Booking booking in _unitOfWork.BookingRepository.GetAll())
            {
                if (booking.Accommodation == _unitOfWork.AccommodationRepository.Read(id))
                {
                    try
                    {
                        DeleteBooking(booking.Id);
                    }
                    catch (Exception)
                    {
                        throw new CancelBookingException("accommodation", booking.Id, booking.Accommodation.CancellationDeadlineInDays);
                    }
                }
            }
        }

        public void DeleteBooking(int id)
        {
            Booking booking = _unitOfWork.BookingRepository.Read(id);

            if (booking == null)
                throw new IdNotFoundException("Booking", id);

            if (CorrectUserOrOwnerOrAdminOrEmployee(booking.Accommodation.Owner.Id, booking.BookedBy))
                DeadLineExpiration(id, booking.Accommodation.CancellationDeadlineInDays);
            else
                throw new AccessException();
        }

        private void DeadLineExpiration(int id, int deadlineInDays)
        {
            string firstDateBooked = _unitOfWork.BookingRepository.Read(id).Dates[0];

            if (CancelationDeadlineCheck(firstDateBooked, deadlineInDays))
            {
                ResetAvailableStatusAfterDeletingBooking(id);
                _unitOfWork.BookingRepository.Delete(id);
                _unitOfWork.Commit();
            }
            else
                throw new CancelBookingException(id, deadlineInDays);
        }

        private void ResetAvailableStatusAfterDeletingBooking(int id)
        {
            Booking booking = _unitOfWork.BookingRepository.Read(id);
            ResetDatesToAvailable(booking.Dates, booking.Accommodation.Schedule);
            _unitOfWork.AccommodationRepository.Update(booking.Accommodation.Id, booking.Accommodation);
        }
    }
}