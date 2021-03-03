using AintBnB.Core.Models;
using System.Collections.Generic;

namespace AintBnB.BusinessLogic.Interfaces
{
    public interface IAccommodationService
    {
        void ValidateAccommodation(Accommodation accommodation);
        Accommodation CreateAccommodation(User owner, Address address, int squareMeters, int amountOfBedroooms, double kilometersFromCenter, string description, int pricePerNight, int cancellationDeadlineInDays, List<byte[]> picture, int daysToCreateScheduleFor);
        Accommodation GetAccommodation(int id);
        List<Accommodation> GetAllAccommodations();
        List<Accommodation> GetAllOwnedAccommodations(int userid);
        void UpdateAccommodation(int id, Accommodation accommodation);
        public void ExpandScheduleOfAccommodationWithXAmountOfDays(int id, int days);
        List<Accommodation> FindAvailable(string country, string city, string startdate, int nights);
        List<Accommodation> SortListOfAccommodations(List<Accommodation> available, string sortBy, string ascOrDesc);
    }
}
