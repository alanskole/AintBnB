using System;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class AccessException : Exception
    {
        public AccessException(string message) : base(message)
        {
        }
    }
}
