using AintBnB.BusinessLogic.Imp;
using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.Repository.Imp;
using AintBnB.Repository.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        protected User userAdmin;
        protected User userEmployee1;
        protected User userRequestToBecomeEmployee;
        protected User userRequestToBecomeEmployee2;
        protected User userCustomer1;
        protected User userCustomer2;
        protected User userCustomer3;
        protected Address adr1 = new Address("str", "1", "1111", "ar", "Fredrikstad", "Norway");
        protected Address adr2 = new Address("anotherstr", "10A", "1414", "frstd", "Fredrikstad", "Norway");
        protected Address adr3 = new Address("capitalstr", "42", "0531", "osl", "Oslo", "Norway");
        protected Address adr4 = new Address("blabla", "4232", "34343", "hld", "Halden", "Norway");
        protected Accommodation accommodation1;
        protected Accommodation accommodation2;
        protected Accommodation accommodation3;
        protected Accommodation accommodation4;
        protected Booking booking1;
        protected Booking booking2;
        protected Booking booking3;
        protected Booking booking4;
        protected Booking booking5;
        protected Booking booking6;

        public async Task SetupDatabaseForTestingAsync()
        {
            var sqlconnection = new SqliteConnection("Data Source=:memory:");
            await sqlconnection.OpenAsync();

            var options = new DbContextOptionsBuilder<DatabaseContext>()
                    .UseSqlite(sqlconnection)
                    .Options;

            connection = new DatabaseContext(options);
            await connection.Database.EnsureCreatedAsync();
        }

        public void SetupTestClasses()
        {
            unitOfWork = new UnitOfWork(connection);
            accommodationService = new AccommodationService(unitOfWork);
            bookingService = new BookingService(unitOfWork);
            deletionService = new DeletionService(unitOfWork);
            userService = new UserService(unitOfWork);
        }

        public async Task CreateDummyUsersAsync()
        {
            userAdmin = new User
            {
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

            userCustomer3 = new User
            {
                UserName = "customer3",
                Password = HashPassword("aaaaaa"),
                FirstName = "Third",
                LastName = "Customer",
                UserType = UserTypes.Customer
            };

            await unitOfWork.UserRepository.CreateAsync(userAdmin);
            await connection.SaveChangesAsync();
            await unitOfWork.UserRepository.CreateAsync(userEmployee1);
            await connection.SaveChangesAsync();
            await unitOfWork.UserRepository.CreateAsync(userRequestToBecomeEmployee);
            await connection.SaveChangesAsync();
            await unitOfWork.UserRepository.CreateAsync(userRequestToBecomeEmployee2);
            await connection.SaveChangesAsync();
            await unitOfWork.UserRepository.CreateAsync(userCustomer1);
            await connection.SaveChangesAsync();
            await unitOfWork.UserRepository.CreateAsync(userCustomer2);
            await connection.SaveChangesAsync();
            await unitOfWork.UserRepository.CreateAsync(userCustomer3);
            await connection.SaveChangesAsync();
        }

        public async Task CreateDummyAccommodationAsync()
        {
            var schedule = new SortedDictionary<string, bool>();

            for (int i = 0; i < 100; i++)
            {
                schedule.Add(DateTime.Today.AddDays(i).ToString("yyyy-MM-dd"), true);
            }

            accommodation1 = new Accommodation
            {
                Owner = userCustomer1,
                Address = adr1,
                SquareMeters = 50,
                AmountOfBedrooms = 1,
                KilometersFromCenter = 1.2,
                Description = "blah blaaaaah",
                PricePerNight = 500,
                CancellationDeadlineInDays = 1,
                Schedule = schedule,
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
            };

            accommodation4 = new Accommodation
            {
                Owner = userCustomer3,
                Address = adr4,
                SquareMeters = 60,
                AmountOfBedrooms = 2,
                KilometersFromCenter = 4.8,
                Description = "bla bla",
                PricePerNight = 590,
                CancellationDeadlineInDays = 1,
                Schedule = new SortedDictionary<string, bool>(schedule),
            };

            await unitOfWork.AccommodationRepository.CreateAsync(accommodation1);
            await connection.SaveChangesAsync();
            await unitOfWork.AccommodationRepository.CreateAsync(accommodation2);
            await connection.SaveChangesAsync();
            await unitOfWork.AccommodationRepository.CreateAsync(accommodation3);
            await connection.SaveChangesAsync();
            await unitOfWork.AccommodationRepository.CreateAsync(accommodation4);
            await connection.SaveChangesAsync();
        }

        public async Task CreateDummyBookingAsync()
        {
            var bkdt = DateTime.Today.AddDays(2);
            var past = DateTime.Today.AddDays(-20);

            var dates1 = new List<string>();

            for (int i = 0; i < 5; i++)
            {
                var dt = bkdt.AddDays(i);
                accommodation1.Schedule[dt.ToString("yyyy-MM-dd")] = false;
                dates1.Add(dt.ToString("yyyy-MM-dd"));
            }

            var dates2 = new List<string>();

            for (int i = 3; i < 15; i++)
            {
                var dt = bkdt.AddDays(i);
                accommodation2.Schedule[dt.ToString("yyyy-MM-dd")] = false;
                dates2.Add(dt.ToString("yyyy-MM-dd"));
            }

            var dates3 = new List<string>();

            for (int i = 0; i < 4; i++)
            {
                var dt = DateTime.Today.AddDays(i + 2);
                accommodation3.Schedule[dt.ToString("yyyy-MM-dd")] = false;
                dates3.Add(dt.ToString("yyyy-MM-dd"));
            }

            var dates4 = new List<string>();

            for (int i = 0; i < 5; i++)
            {
                var dt = past.AddDays(i);
                dates4.Add(dt.ToString("yyyy-MM-dd"));
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

            booking4 = new Booking
            {
                BookedBy = userCustomer3,
                Accommodation = accommodation1,
                Dates = dates4,
                Price = (accommodation1.PricePerNight * dates4.Count)
            };

            booking5 = new Booking
            {
                BookedBy = userCustomer2,
                Accommodation = accommodation4,
                Dates = dates2,
                Price = (accommodation1.PricePerNight * dates2.Count)
            };

            booking6 = new Booking
            {
                BookedBy = userCustomer3,
                Accommodation = accommodation1,
                Dates = dates2,
                Price = (accommodation1.PricePerNight * dates2.Count)
            };

            await unitOfWork.BookingRepository.CreateAsync(booking1);
            await connection.SaveChangesAsync();
            await unitOfWork.BookingRepository.CreateAsync(booking2);
            await connection.SaveChangesAsync();
            await unitOfWork.BookingRepository.CreateAsync(booking3);
            await connection.SaveChangesAsync();
            await unitOfWork.BookingRepository.CreateAsync(booking4);
            await connection.SaveChangesAsync();
            await unitOfWork.BookingRepository.CreateAsync(booking5);
            await connection.SaveChangesAsync();
            await unitOfWork.BookingRepository.CreateAsync(booking6);
            await connection.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
            await connection.Database.EnsureDeletedAsync();
        }
    }
}