using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using Microsoft.EntityFrameworkCore;

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
