using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace Test
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Startup>
    {
        public User userAdmin;
        public User userCustomer1;
        public User userCustomer2;
        public Address adr = new Address("str", "1", "1111", "ar", "Fredrikstad", "Norway");
        public Address adr2 = new Address("anotherstr", "10A", "1414", "frstd", "Fredrikstad", "Norway");
        public Address adr3 = new Address("capitalstr", "42", "0531", "osl", "Oslo", "Norway");
        public Accommodation accommodation1;
        public Accommodation accommodation2;
        public Accommodation accommodation3;
        public Booking booking1;
        public Booking booking2;
        public Booking booking3;
        public DatabaseContext db;
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<DatabaseContext>));

                services.Remove(descriptor);

                services.AddDbContext<DatabaseContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                services.AddMvc(options =>
                {
                    options.Filters.Add(new IgnoreAntiforgeryTokenAttribute());
                });

                var sp = services.BuildServiceProvider();

                var scope = sp.CreateScope();

                var scopedServices = scope.ServiceProvider;
                db = scopedServices.GetRequiredService<DatabaseContext>();

                db.Database.EnsureCreated();

                userAdmin = new User
                {
                    UserName = "admin",
                    Password = HashPassword("aaaaaa"),
                    FirstName = "Ad",
                    LastName = "Min",
                    UserType = UserTypes.Admin
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

                db.Add(userAdmin);
                db.SaveChanges();
                db.Add(userCustomer1);
                db.SaveChanges();
                db.Add(userCustomer2);
                db.SaveChanges();

                var schedule = new SortedDictionary<string, bool>();

                for (int i = 0; i < 100; i++)
                {
                    schedule.Add(DateTime.Today.AddDays(i).ToString("yyyy-MM-dd"), true);
                }

                db.Add(adr);
                db.SaveChanges();
                db.Add(adr2);
                db.SaveChanges();
                db.Add(adr3);
                db.SaveChanges();

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
                    CancellationDeadlineInDays = 1,
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
                    CancellationDeadlineInDays = 1,
                    Schedule = new SortedDictionary<string, bool>(schedule),
                };

                db.Add(accommodation1);
                db.SaveChanges();
                db.Add(accommodation2);
                db.SaveChanges();
                db.Add(accommodation3);
                db.SaveChanges();

                var bkdt = DateTime.Today.AddDays(2);

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

                db.Add(booking1);
                db.SaveChanges();
                db.Add(booking2);
                db.SaveChanges();
                db.Add(booking3);
                db.SaveChanges();
            });
        }

        public async Task LoginUserAsync(HttpClient client, string[] info)
        {
            await client.PostAsync("api/authentication/login",
                new StringContent(JsonConvert.SerializeObject(info), Encoding.UTF8, "application/json"));
        }

        public void DisposeDb()
        {
            db.Database.EnsureDeleted();
        }
    }
}
