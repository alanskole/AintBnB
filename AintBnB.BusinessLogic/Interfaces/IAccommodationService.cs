using AintBnB.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AintBnB.BusinessLogic.Interfaces
{
    public interface IAccommodationService
    {
        Task ValidateAccommodationAsync(Accommodation accommodation);
        Task<Accommodation> CreateAccommodationAsync(User owner, Address address, int squareMeters, int amountOfBedroooms, double kilometersFromCenter, string description, int pricePerNight, int cancellationDeadlineInDays, List<byte[]> picture, int daysToCreateScheduleFor);
        Task<Accommodation> GetAccommodationAsync(int id);
        Task<List<Accommodation>> GetAllAccommodationsAsync();
        Task<List<Accommodation>> GetAllOwnedAccommodationsAsync(int userid);
        Task UpdateAccommodationAsync(int id, Accommodation accommodation);
        Task ExpandScheduleOfAccommodationWithXAmountOfDaysAsync(int id, int days);
        Task<List<Accommodation>> FindAvailableAsync(string country, string city, string startdate, int nights);
        List<Accommodation> SortListOfAccommodations(List<Accommodation> available, string sortBy, string ascOrDesc);
    }
}
