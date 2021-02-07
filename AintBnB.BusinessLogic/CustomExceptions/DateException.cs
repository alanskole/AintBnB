using System;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class DateException : Exception
    {
        public DateException(string cause) : base(cause)
        {
        }
    }
}
