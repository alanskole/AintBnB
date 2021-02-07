using System;
using System.Collections.Generic;
using System.Text;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class NoneFoundInDatabaseTableException : Exception
    {
        public NoneFoundInDatabaseTableException(string type) : base($"No {type} found in the database!")
        {
        }
    }
}
