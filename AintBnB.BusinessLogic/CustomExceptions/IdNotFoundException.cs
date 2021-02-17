using System;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class IdNotFoundException : Exception
    {
        public IdNotFoundException(string type) : base($"{type} not found in the system!")
        {
        }

        public IdNotFoundException(string type, int id) : base($"{type} with ID {id} not found!")
        {
        }
    }
}
