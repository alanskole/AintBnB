using System;
using System.Collections.Generic;
using System.Text;

namespace AintBnB.BusinessLogic.CustomExceptions
{
    [Serializable]
    public class GeographicalException : Exception
    {
        public GeographicalException() : base($"No European countries found in the database!")
        {
        }

        public GeographicalException(string country) : base($"{country} does not exist in Europe!")
        {
        }

        public GeographicalException(string country, string city) : base($"{city} not found in {country}!")
        {
        }
    }
}
