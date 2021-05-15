using AintBnB.Core.Models;
using System.Threading.Tasks;

namespace AintBnB.Repository.Interfaces
{
    public interface IUnitOfWork
    {
        IRepository<Accommodation> AccommodationRepository { get; }
        IRepository<Booking> BookingRepository { get; }
        IImageRepository ImageRepository { get; }

        IRepository<User> UserRepository { get; }
        Task CommitAsync();
    }
}
