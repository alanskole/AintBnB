using System;
using System.Collections.Generic;
using System.Text;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class EmptyFieldsException : Exception
    {
        public EmptyFieldsException() : base("None of the fields can be empty!")
        {
        }
    }
}
