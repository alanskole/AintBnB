using AintBnB.Core.Models;

namespace AintBnB.Repository.Interfaces
{
    public interface IUnitOfWork
    {
        IRepository<Accommodation> AccommodationRepository { get; }
        IRepository<Booking> BookingRepository { get; }
        IRepository<User> UserRepository { get; }
        void Commit();
    }
}
