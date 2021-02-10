using System;
using System.Collections.Generic;
using System.Text;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class AccessException : Exception
    {
        public AccessException() : base("Administrator only!")
        {
        }

        public AccessException(int id) : base($"Administrator or user with ID {id} only!")
        {
        }

        public AccessException(int idOnwer, int idUser) : base($"Administrator or user with ID {idOnwer} or {idUser} only!")
        {
        }
    }
}
