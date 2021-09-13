using System;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class NotFoundException : Exception
    {
        public NotFoundException(string type, int id) : base($"{type} with ID {id} not found!")
        {
        }

        public NotFoundException(int id, string type) : base($"User with Id {id} doesn't have any {type}!")
        {
        }
        public NotFoundException(string type) : base($"No {type} found!")
        {
        }
    }
}
