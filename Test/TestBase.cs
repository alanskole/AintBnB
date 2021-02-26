using AintBnB.BusinessLogic.Imp;
using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.Repository.Imp;
using AintBnB.Repository.Interfaces;
using AintBnB.WebApi.Controllers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net.Http;
using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace Test
{
    public class TestBase
    {
        protected DatabaseContext connection;
        protected IUnitOfWork unitOfWork;
        protected IAccommodationService accommodationService;
        protected IBookingService bookingService;
        protected IDeletionService deletionService;
        protected IUserService userService;
        protected AccommodationController accommodationController;
        protected AuthenticationController authenticationController;
        protected BookingController bookingController;
        protected UserController userController;
        protected User userAdmin;
        protected User userEmployee1;
        protected User userRequestToBecomeEmployee;
        protected User userRequestToBecomeEmployee2;
        protected User userCustomer1;
        protected User userCustomer2;
        protected Address adr = new Address("str", "1", "1111", "ar", "Fredrikstad", "Norway");
        protected Address adr2 = new Address("anotherstr", "10A", "1414", "frstd", "Fredrikstad", "Norway");
        protected Address adr3 = new Address("capitalstr", "42", "0531", "osl", "Oslo", "Norway");
        protected Accommodation accommodation1;
        protected Accommodation accommodation2;
        protected Accommodation accommodation3;
        protected Booking booking1;
        protected Booking booking2;
        protected Booking booking3;
        protected string LocalHostAddress = "https://localhost:";
        protected string LocalHostPort = "44342/";
        protected string ControllerPartOfUri;
        protected HttpClientHandler clientHandler;
        protected HttpClient client;
        protected string uri;
        protected string uniquePartOfUri;

        public void SetupDatabaseForTesting()
        {
            var sqlconnection = new SqliteConnection("Data Source=:memory:");
            sqlconnection.Open();

            var options = new DbContextOptionsBuilder<DatabaseContext>()
                    .UseSqlite(sqlconnection)
                    .Options;

            connection = new DatabaseContext(options);
            connection.Database.EnsureCreated();
        }

        public void SetupTestClasses()
        {
            unitOfWork = new UnitOfWork(connection);
            accommodationService = new AccommodationService(unitOfWork);
            bookingService = new BookingService(unitOfWork);
            deletionService = new DeletionService(unitOfWork);
            userService = new UserService(unitOfWork);
            accommodationController = new AccommodationController(accommodationService, userService, deletionService);
            authenticationController = new AuthenticationController(userService);
            bookingController = new BookingController(bookingService, userService, accommodationService, deletionService);
            userController = new UserController(userService, deletionService);
        }

        public void SetupHttp(string ctrStr)
        {
            clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            client = new HttpClient(clientHandler);
            ControllerPartOfUri = "api/" + ctrStr + "/";
            uri = LocalHostAddress + LocalHostPort + ControllerPartOfUri;
        }

        public void CreateDummyUsers()
        {
            userAdmin = new User { 
                UserName = "admin", 
                Password = HashPassword("aaaaaa"), 
                FirstName = "Ad", 
                LastName = "Min", 
                UserType = UserTypes.Admin
            };

            userEmployee1 = new User
            {
                UserName = "employee1",
                Password = HashPassword("aaaaaa"),
                FirstName = "Emp",
                LastName = "Loyee",
                UserType = UserTypes.Employee
            };

            userRequestToBecomeEmployee = new User
            {
                UserName = "empreq",
                Password = HashPassword("aaaaaa"),
                FirstName = "Wannabe",
                LastName = "Employee",
                UserType = UserTypes.RequestToBeEmployee
            };

            userRequestToBecomeEmployee2 = new User
            {
                UserName = "anotherempreq",
                Password = HashPassword("aaaaaa"),
                FirstName = "Letmebe",
                LastName = "Loyeeemp",
                UserType = UserTypes.RequestToBeEmployee
            };

            userCustomer1 = new User
            {
                UserName = "customer1",
                Password = HashPassword("aaaaaa"),
                FirstName = "First",
                LastName = "Customer",
                UserType = UserTypes.Customer
            };

            userCustomer2 = new User
            {
                UserName = "customer2",
                Password = HashPassword("aaaaaa"),
                FirstName = "Second",
                LastName = "Customer",
                UserType = UserTypes.Customer
            };

            unitOfWork.UserRepository.Create(userAdmin);
            unitOfWork.UserRepository.Create(userEmployee1);
            unitOfWork.UserRepository.Create(userRequestToBecomeEmployee);
            unitOfWork.UserRepository.Create(userRequestToBecomeEmployee2);
            unitOfWork.UserRepository.Create(userCustomer1);
            unitOfWork.UserRepository.Create(userCustomer2);
            connection.SaveChanges();
        }

        public void CreateDummyAccommodation()
        {
            SortedDictionary<string, bool> schedule = new SortedDictionary<string, bool>();

            for (int i = 0; i < 100; i++)
            {
                schedule.Add(DateTime.Today.AddDays(i).ToString("yyyy-MM-dd"), true);
            }

            connection.Address.Add(adr);
            connection.Address.Add(adr2);

            accommodation1 = new Accommodation
            {
                Owner = userCustomer1,
                Address = adr,
                SquareMeters = 50,
                AmountOfBedrooms = 1,
                KilometersFromCenter = 1.2,
                Description = "blah blaaaaah",
                PricePerNight = 500,
                CancellationDeadlineInDays = 1,
                Schedule = schedule,
                Picture = new List<byte[]>()
            };

            accommodation2 = new Accommodation
            {
                Owner = userCustomer2,
                Address = adr2,
                SquareMeters = 75,
                AmountOfBedrooms = 3,
                KilometersFromCenter = 10.1,
                Description = "cool stuff",
                PricePerNight = 900,
                CancellationDeadlineInDays = 5,
                Schedule = new SortedDictionary<string, bool>(schedule),
                Picture = new List<byte[]>()
            };

            accommodation3 = new Accommodation
            {
                Owner = userCustomer2,
                Address = adr3,
                SquareMeters = 35,
                AmountOfBedrooms = 0,
                KilometersFromCenter = 0.8,
                Description = "cozy and central",
                PricePerNight = 1400,
                CancellationDeadlineInDays = 3,
                Schedule = new SortedDictionary<string, bool>(schedule),
                Picture = new List<byte[]>()
            };

            unitOfWork.AccommodationRepository.Create(accommodation1);
            unitOfWork.AccommodationRepository.Create(accommodation2);
            unitOfWork.AccommodationRepository.Create(accommodation3);
            connection.SaveChanges();
        }

        public void CreateDummyBooking()
        {
            DateTime bkdt = DateTime.Today.AddDays(2);

            List<string> dates1 = new List<string>();

            for (int i = 0; i < 5; i++)
            {
                DateTime dt = bkdt.AddDays(i);
                dates1.Add(dt.ToString("yyyy-MM-dd"));
            }

            List<string> dates2 = new List<string>();

            for (int i = 5; i < 15; i++)
            {
                DateTime dt = bkdt.AddDays(i);
                dates2.Add(dt.ToString("yyyy-MM-dd"));
            }

            List<string> dates3 = new List<string>();

            for (int i = 0; i < 4; i++)
            {
                DateTime dt = DateTime.Today.AddDays(i+2);
                dates3.Add(dt.ToString("yyyy-MM-dd"));
            }

            booking1 = new Booking
            {
                BookedBy = userCustomer2,
                Accommodation = accommodation1,
                Dates = dates1,
                Price = (accommodation1.PricePerNight * dates1.Count)
            };

            booking2 = new Booking
            {
                BookedBy = userCustomer1,
                Accommodation = accommodation2,
                Dates = dates2,
                Price = (accommodation2.PricePerNight * dates2.Count)
            };

            booking3 = new Booking
            {
                BookedBy = userCustomer1,
                Accommodation = accommodation3,
                Dates = dates3,
                Price = (accommodation3.PricePerNight * dates3.Count)
            };

            unitOfWork.BookingRepository.Create(booking1);
            unitOfWork.BookingRepository.Create(booking2);
            unitOfWork.BookingRepository.Create(booking3);
            connection.SaveChanges();
        }
        
        public void Dispose()
        {
            connection.Database.EnsureDeleted();
        }
    }
}
