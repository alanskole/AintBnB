using System;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class AlreadyLoggedInException : Exception
    {
        public AlreadyLoggedInException() : base("Already logged in!")
        {

        }
    }
}
