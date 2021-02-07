using AintBnB.Core.Models;
using System.Collections.Generic;

namespace AintBnB.BusinessLogic.Services
{
    public interface IAccommodationService
    {
        void ValidateAccommodation(Accommodation accommodation);
        Accommodation CreateAccommodation(User owner, Address address, /*string street, int number, int zip, string area, string municipalitiy,*/ int squareMeters, int amountOfBedroooms, double kilometersFromCenter, string description, int pricePerNight, int daysToCreateScheduleFor);
        Accommodation GetAccommodation(int id);
        List<Accommodation> GetAllAccommodations();
        void UpdateAccommodation(int id, Accommodation updatedAccommodation);
        public void ExpandScheduleOfAccommodationWithXAmountOfDays(int id, int days);
        List<Accommodation> FindAvailable(string country, string municipality, string startdate, int nights);
    }
}
