using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.BusinessLogic.Repository;
using AintBnB.BusinessLogic.Services;
using System;
using System.Net.Http;

namespace AintBnB.BusinessLogic.DependencyProviderFactory
{
    public class ProvideDependencyFactory
    {
        public static readonly HttpClient client = new HttpClient();
        public static readonly DatabaseContext databaseContext = new DatabaseContext();

        public static readonly IRepository<User> userRepository = new UserRepository();
        public static readonly IRepository<Booking> bookingRepository = new BookingRepository();
        public static readonly IRepository<Accommodation> accommodationRepository = new AccommodationRepository();

        public static readonly IUserService userService = new UserService(userRepository);
        public static readonly IAccommodationService accommodationService = new AccommodationService(accommodationRepository);
        public static readonly IBookingService bookingService = new BookingService(bookingRepository);
        public static readonly IDeletionService deletionService = new DeletionService(userRepository, accommodationRepository, bookingRepository);

        private HttpClient _clientTesting;
        private IRepository<User> _userRepositoryTesting;
        private IRepository<Booking> _bookingRepositoryTesting;
        private IRepository<Accommodation> _accommodationRepositoryTest;

        public HttpClient HttpClientTesting
        {
            get { return _clientTesting; }
            set
            {
                if (value == null)
                    throw new ArgumentException("HttpClientTesting name cannot be zero");
                _clientTesting = value;
            }
        }

        public IRepository<User> IUserRepositoryTesting
        {
            get { return _userRepositoryTesting; }
            set
            {
                if (value == null)
                    throw new ArgumentException("IUserRepositoryTesting name cannot be zero");
                _userRepositoryTesting = value;
            }
        }

        public IRepository<Booking> IBookingRepositoryTesting
        {
            get { return _bookingRepositoryTesting; }
            set
            {
                if (value == null)
                    throw new ArgumentException("IBookingRepositoryTesting name cannot be zero");
                _bookingRepositoryTesting = value;
            }
        }

        public IRepository<Accommodation> IAccommodationRepositoryTesting
        {
            get { return _accommodationRepositoryTest; }
            set
            {
                if (value == null)
                    throw new ArgumentException("IAccommodationRepositoryTesting name cannot be zero");
                _accommodationRepositoryTest = value;
            }
        }
    }
}
