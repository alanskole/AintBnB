using System.Threading.Tasks;

namespace AintBnB.BusinessLogic.Interfaces
{
    public interface IDeletionService
    {
        /// <summary>Deletes an accommodation.</summary>
        /// <param name="id">The ID of the accommodation to be deleted.</param>
        Task DeleteAccommodationAsync(int id);

        /// <summary>Deletes a booking.</summary>
        /// <param name="id">The ID of the booking to be deleted.</param>
        Task DeleteBookingAsync(int id);

        /// <summary>Delete a user.</summary>
        /// <param name="id">The user-ID of the user to delete.</param>
        Task DeleteUserAsync(int id);
    }
}
