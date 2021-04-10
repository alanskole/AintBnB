using System;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class CancelBookingException : Exception
    {
        public CancelBookingException(int id, int deadlineInDays) : base($"Cannot change the booking with ID {id} because the start date of the booking is less than {deadlineInDays} days away!")
        {
        }

        public CancelBookingException(string type, int id, int deadlineInDays) : base($"The {type} cannot be deleted because it has a booking with ID {id} with a start date less than {deadlineInDays} days away! DeleteAsync when no bookings are less than {deadlineInDays} days away.")
        {
        }
    }
}
