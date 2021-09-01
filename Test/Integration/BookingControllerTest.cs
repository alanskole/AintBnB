using AintBnB.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace Test.Integration
{
    [TestClass]
    public class BookingControllerTest
    {
        private static CustomWebApplicationFactory _factory;
        private HttpClient _client;

        [TestInitialize]
        public async Task SetUpAsync()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
            await _factory.LoginUserAsync(_client, new string[] { _factory.userAdmin.UserName, "aaaaaa" });
        }

        [TestCleanup]
        public void TearDown()
        {
            _factory.DisposeDb();
        }

        [TestMethod]
        public async Task Book_ShouldReturn_SuccessStatus()
        {
            string startDate = _factory.accommodation1.Schedule.Keys.Last();

            var bookingInfo = JsonConvert.SerializeObject(new string[] { startDate, "3", "1", "1" });

            var response = await _client.PostAsync(
                "api/booking/book", new StringContent(bookingInfo, Encoding.UTF8, "application/json"));

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public async Task Book_ShouldReturn_BadRequestIfError()
        {
            string startDate = _factory.accommodation1.Schedule.Keys.Last();

            var bookingInfo = JsonConvert.SerializeObject(new string[] { startDate, "6", "2", "1" });

            var response = await _client.PostAsync(
                "api/booking/book", new StringContent(bookingInfo, Encoding.UTF8, "application/json"));
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task UpdateBooking_ShouldReturn_SuccessStatus()
        {
            string startDate = _factory.accommodation1.Schedule.Keys.Last();

            var newDates = JsonConvert.SerializeObject(new string[] { startDate, "1" });

            var response = await _client.PutAsync("api/booking/1", new StringContent(newDates, Encoding.UTF8, "application/json"));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task UpdateBooking_ShouldReturn_BadRequestIfError()
        {
            string startDate = _factory.accommodation1.Schedule.Keys.Last();

            var newDates = JsonConvert.SerializeObject(new string[] { startDate, "2" });

            var response = await _client.PutAsync("api/booking/1", new StringContent(newDates, Encoding.UTF8, "application/json"));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetBooking_ShouldReturn_SuccessStatus()
        {
            var response = await _client.GetAsync("api/booking/1");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetBooking_ShouldReturn_NotFoundIfError()
        {
            var response = await _client.GetAsync("api/booking/100");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetBookingsOnOwnedAccommodations_ShouldReturn_SuccessStatus()
        {
            var response = await _client.GetAsync("api/booking/3/bookingsownaccommodation");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetBookingsOnOwnedAccommodations_ShouldReturn_NotFoundIfError()
        {
            var userCustomer3 = new User
            {
                UserName = "customer3",
                Password = HashPassword("aaaaaa"),
                FirstName = "Second",
                LastName = "Customer",
                UserType = UserTypes.Customer
            };

            _factory.db.Add(userCustomer3);
            _factory.db.SaveChanges();

            var response = await _client.GetAsync("api/booking/4/bookingsownaccommodation");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetAllBookings_ShouldReturn_SuccessStatus()
        {
            var response = await _client.GetAsync("api/booking");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task DeleteBooking_ShouldReturn_SuccessStatus()
        {
            var response = await _client.DeleteAsync("api/booking/1");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteBooking_ShouldReturn_BadRequestIfError()
        {
            var response = await _client.DeleteAsync("api/booking/100");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }
    }
}
