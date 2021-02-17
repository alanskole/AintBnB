using System;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class PasswordChangeException : Exception
    {
        public PasswordChangeException() : base("The new and old password must be different!")
        {
        }
        public PasswordChangeException(string newOrOld) : base($"The {newOrOld} passwords don't match!")
        {
        }
    }
}
