using System;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class LoginExcrption : Exception
    {
        public LoginExcrption(string cause) : base(cause)
        {
        }
    }
}
