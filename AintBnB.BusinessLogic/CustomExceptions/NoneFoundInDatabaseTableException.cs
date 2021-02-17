using System;
using System.Collections.Generic;
using System.Text;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class NoneFoundInDatabaseTableException : Exception
    {
        public NoneFoundInDatabaseTableException(int id, string type) : base($"User with Id {id} doesn't have any {type}!")
        {
        }
        public NoneFoundInDatabaseTableException(string type) : base($"No {type} found!")
        {
        }
    }
}
