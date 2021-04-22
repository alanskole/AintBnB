using System;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class CancelBookingException : Exception
    {
        public CancelBookingException(int id, int deadlineInDays) : base($"Cannot change the booking with ID {id} because the start date of the booking is less than {deadlineInDays} days away!")
        {
        }

        public CancelBookingException(string type, int id, int deadlineInDays) : base($"The {type} cannot be deleted because it has a booking with ID {id} with a start date less than {deadlineInDays} days away! Delete when no bookings are less than {deadlineInDays} days away.")
        {
        }

        public CancelBookingException(int[] numbers) : base($"The user with ID {numbers[0]} cannot be deleted because it has an accommodation with ID {numbers[1]} that has bookings that can't be deleted because of the cancellation deadline of the accommodation. Delete when no bookings of the accommodation have surpassed the cancellation deadline of {numbers[2]} days.")
        {
        }
    }
}
