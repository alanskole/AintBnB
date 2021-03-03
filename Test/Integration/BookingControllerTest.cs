using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AintBnB.Core.Models;
using AintBnB.WebApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace Test.Integration
{
    [TestFixture]
    public class BookingControllerTest
    {
        private static CustomWebApplicationFactory _factory;
        private HttpClient _client;

        [SetUp]
        public void SetUp()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _factory.DisposeDb();
        }

        [Test]
        public async Task Book_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = _factory.userAdmin;

            string startDate = _factory.accommodation1.Schedule.Keys.Last();

            var response = await _client.GetAsync("api/booking/" + startDate + "/6/1/1");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task Book_ShouldReturn_BadRequestIfError()
        {
            LoggedInAs = _factory.userAdmin;

            string startDate = _factory.accommodation1.Schedule.Keys.Last();

            var response = await _client.GetAsync("api/booking/" + startDate + "/6/2/1");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task UpdateBooking_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = _factory.userAdmin;

            string startDate = _factory.accommodation1.Schedule.Keys.Last();

            var response = await _client.GetAsync("api/booking/" + startDate + "/1/1");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task UpdateBooking_ShouldReturn_BadRequestIfError()
        {
            LoggedInAs = _factory.userAdmin;

            string startDate = _factory.accommodation1.Schedule.Keys.Last();

            var response = await _client.GetAsync("api/booking/" + startDate + "/2/1");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task GetBooking_ShouldReturn_SuccessStatus()
        {

            LoggedInAs = _factory.userAdmin;

            var response = await _client.GetAsync("api/booking/1");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task GetBooking_ShouldReturn_NotFoundIfError()
        {

            LoggedInAs = _factory.userAdmin;

            var response = await _client.GetAsync("api/booking/100");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task GetBookingsOnOwnedAccommodations_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = _factory.userCustomer2;

            var response = await _client.GetAsync("api/booking/6/bookingsownaccommodation");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task GetBookingsOnOwnedAccommodations_ShouldReturn_NotFoundIfError()
        {
            LoggedInAs = _factory.userCustomer2;

            var response = await _client.GetAsync("api/booking/600/bookingsownaccommodation");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task GetAllBookings_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = _factory.userAdmin;

            var response = await _client.GetAsync("api/booking");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task GetAllBookings_ShouldReturn_NotFoundIfError()
        {
            LoggedInAs = null;

            var response = await _client.GetAsync("api/booking");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task DeleteBooking_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = _factory.userAdmin;

            var response = await _client.DeleteAsync("api/booking/1");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task DeleteBooking_ShouldReturn_BadRequestIfError()
        {
            LoggedInAs = _factory.userAdmin;

            var response = await _client.DeleteAsync("api/booking/100");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }
    }
}
