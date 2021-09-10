using AintBnB.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Test.Integration
{
    [TestClass]
    public class AccommodationControllerTest
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
        public async Task CreateAccommodation_ShouldReturn_SuccessStatus()
        {
            Address addr = new Address
            {
                Id = 500,
                Street = _factory.adr.Street,
                Number = _factory.adr.Number,
                Zip = _factory.adr.Zip,
                Area = _factory.adr.Area,
                City = _factory.adr.City,
                Country = _factory.adr.Country
            };

            Accommodation ac = new Accommodation
            {
                Address = addr,
                SquareMeters = 50,
                AmountOfBedrooms = 1,
                KilometersFromCenter = 1.2,
                Description = "blah blaaaaah",
                PricePerNight = 500,
                CancellationDeadlineInDays = 1,
                Schedule = _factory.accommodation1.Schedule,
            };

            var response = await _client.PostAsync("api/accommodation/10/3",
                new StringContent(
                    JsonConvert.SerializeObject(ac),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task CreateAccommodation_ShouldReturn_BadRequestIfError()
        {
            Accommodation ac = new Accommodation
            {
            };

            var response = await _client.PostAsync("api/accommodation/10/6",
                new StringContent(
                    JsonConvert.SerializeObject(ac),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task FindAvailable_ShouldReturn_SuccessStatus()
        {
            string city = _factory.accommodation1.Address.City + "/";
            string country = _factory.accommodation1.Address.Country + "/";
            string startDate = _factory.accommodation1.Schedule.Keys.Last() + "/";


            var response = await _client.GetAsync("api/accommodation/" + country + city + startDate + "1");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task FindAvailable_ShouldReturn_NotFoundIfError()
        {
            string city = _factory.accommodation1.Address.City + "/";
            string country = _factory.accommodation1.Address.Country + "/";
            string startDate = _factory.accommodation1.Schedule.Keys.Last() + "/";


            var response = await _client.GetAsync("api/accommodation/" + country + city + startDate + "2");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task SortAvailableList_ShouldReturn_SuccessStatus()
        {
            List<Accommodation> accs = new List<Accommodation>() { _factory.accommodation1, _factory.accommodation2 };

            var response = await _client.PutAsync("api/accommodation/sort/Size/Ascending",
                new StringContent(
                    JsonConvert.SerializeObject(accs),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task SortAvailableList_ShouldReturn_BadRequestError()
        {
            List<Accommodation> accs = new List<Accommodation>();

            var response = await _client.PutAsync("api/accommodation/sort/Size/Ascending",
                new StringContent(
                    JsonConvert.SerializeObject(accs),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task ExpandSchedule_ShouldReturn_SuccessStatus()
        {
            var response = await _client.PutAsync("api/accommodation/1/expand",
                            new StringContent(
                                JsonConvert.SerializeObject(100),
                                Encoding.UTF8,
                                "application/json"));

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task ExpandSchedule_ShouldReturn_BadRequestError()
        {
            _client = _factory.CreateClient();
            await _factory.LoginUserAsync(_client, new string[] { _factory.userCustomer2.UserName, "aaaaaa" });

            var response = await _client.PostAsync("api/accommodation/1/expand",
                new StringContent(
                    JsonConvert.SerializeObject(100),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("application/problem+json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task UpdateAccommodation_ShouldReturn_SuccessStatus()
        {
            _factory.accommodation1.Description = "ssdssddsdsdsasdsdsdsdsd";

            var response = await _client.PutAsync("api/accommodation/1",
                new StringContent(
                    JsonConvert.SerializeObject(_factory.accommodation1),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task UpdateAccommodation_ShouldReturn_BadRequestIfError()
        {
            var response = await _client.PutAsync("api/accommodation/1",
                new StringContent(
                    JsonConvert.SerializeObject(new Accommodation()),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetAllAccommodationsInTheSystem_ShouldReturn_SuccessStatus()
        {
            var response = await _client.GetAsync("api/accommodation");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetAllAccommodationsOfAUser_ShouldReturn_SuccessStatus()
        {
            var response = await _client.GetAsync("api/accommodation/2/allaccommodations");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetAllAccommodationsOfAUser_ShouldReturn_NotFoundIfError()
        {
            var response = await _client.GetAsync("api/accommodation/5/allaccommodations");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetAccommodation_ShouldReturn_SuccessStatus()
        {
            var response = await _client.GetAsync("api/accommodation/1");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetAccommodation_ShouldReturn_NotFoundIfError()
        {
            var response = await _client.GetAsync("api/accommodation/100");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task DeleteAccommodation_ShouldReturn_SuccessStatus()
        {
            var response = await _client.DeleteAsync("api/accommodation/1");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteAccommodation_ShouldReturn_BadRequestIfError()
        {
            var response = await _client.DeleteAsync("api/accommodation/100");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }
    }
}
