using System;
using System.Collections.Generic;
using System.Text;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class CancelBookingException : Exception
    {
        public CancelBookingException(int id) : base($"Cannot delete the booking with ID {id} because the start date of the booking is less than five days away!")
        {
        }

        public CancelBookingException(string type, int id) : base($"The {type} cannot be deleted because it has a booking with ID {id} with a start date less than five days away! Delete when no bookings are less than five days away.")
        {
        }
    }
}
