using System.Threading.Tasks;

namespace AintBnB.BusinessLogic.Interfaces
{
    public interface IDeletionService
    {
        Task DeleteAccommodationAsync(int id);
        Task DeleteBookingAsync(int id);
        Task DeleteUserAsync(int id);
    }
}
