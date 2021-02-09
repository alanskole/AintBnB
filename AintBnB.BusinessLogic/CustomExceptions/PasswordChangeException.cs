using System;
using System.Collections.Generic;
using System.Text;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class PasswordException : Exception
    {
        public PasswordException() : base("The new and old password must be different!")
        {
        }
        public PasswordException(string newOrOld) : base($"The {newOrOld} passwords don't match!")
        {
        }
    }
}
