using System;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class ParameterException : Exception
    {

        public ParameterException(string parameter, string value) : base($"{parameter} cannot be {value}!")
        {
        }
    }
}
