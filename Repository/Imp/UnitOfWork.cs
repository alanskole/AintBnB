using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.Repository.Interfaces;
using System.Threading.Tasks;

namespace AintBnB.Repository.Imp
{
    public class UnitOfWork : IUnitOfWork
    {
        private DatabaseContext _databaseContext;
        private IRepository<Accommodation> _accommodationRepository;
        private IRepository<Booking> _bookingRepository;
        private IRepository<User> _userRepository;

        public IRepository<Accommodation> AccommodationRepository
        {
            get
            {
                if (_accommodationRepository == null)
                    _accommodationRepository = new AccommodationRepository(_databaseContext);

                return _accommodationRepository;
            }
        }

        public IRepository<Booking> BookingRepository
        {
            get
            {
                if (_bookingRepository == null)
                    _bookingRepository = new BookingRepository(_databaseContext);

                return _bookingRepository;
            }
        }

        public IRepository<User> UserRepository
        {
            get
            {
                if (_userRepository == null)
                    _userRepository = new UserRepository(_databaseContext);

                return _userRepository;
            }
        }

        public UnitOfWork(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task CommitAsync()
        {
            await _databaseContext.SaveChangesAsync();
        }
    }
}
