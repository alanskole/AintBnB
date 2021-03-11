using System;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class GeographicalException : Exception
    {
        public GeographicalException() : base($"No countries found in the database!")
        {
        }

        public GeographicalException(string country) : base($"{country} does not exist!")
        {
        }

        public GeographicalException(string country, string city) : base($"{city} not found in {country}!")
        {
        }
    }
}
