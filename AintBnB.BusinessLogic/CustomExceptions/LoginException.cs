using System;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class LoginException : Exception
    {
        public LoginException(string cause) : base(cause)
        {
        }
    }
}
