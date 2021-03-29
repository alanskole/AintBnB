using System;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class PasswordException : Exception
    {
        public PasswordException(string cause) : base($"The password {cause}!")
        {
        }
    }
}
