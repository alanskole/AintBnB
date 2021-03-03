using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using static AintBnB.BusinessLogic.Helpers.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Test
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Startup>
    {
        public User userAdmin;
        public User userEmployee1;
        public User userRequestToBecomeEmployee;
        public User userRequestToBecomeEmployee2;
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
        protected Booking booking3;
        private DatabaseContext db;
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

                db.Add(userAdmin);
                db.SaveChanges();
                db.Add(userEmployee1);
                db.SaveChanges();
                db.Add(userRequestToBecomeEmployee);
                db.SaveChanges();
                db.Add(userRequestToBecomeEmployee2);
                db.SaveChanges();
                db.Add(userCustomer1);
                db.SaveChanges();
                db.Add(userCustomer2);
                db.SaveChanges();

            });
        }

        public void DisposeDb()
        {
            db.Database.EnsureDeleted();
        }
    }
}
