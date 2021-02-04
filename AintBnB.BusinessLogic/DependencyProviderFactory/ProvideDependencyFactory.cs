using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.BusinessLogic.Repository;
using AintBnB.BusinessLogic.Services;

namespace AintBnB.BusinessLogic.DependencyProviderFactory
{
    public class ProvideDependencyFactory
    {
        public static readonly DatabaseContext databaseContext = new DatabaseContext();

        public static readonly IRepository<User> userRepository = new UserRepository();
        public static readonly IRepository<Booking> bookingRepository = new BookingRepository();
        public static readonly IRepository<Accommodation> accommodationRepository = new AccommodationRepository();

        public static readonly IUserService userService = new UserService(userRepository);
        public static readonly IAccommodationService accommodationService = new AccommodationService(accommodationRepository);
        public static readonly IBookingService bookingService = new BookingService(bookingRepository);
        public static readonly IDeletionService deletionService = new DeletionService(userRepository, accommodationRepository, bookingRepository);
    }
}
