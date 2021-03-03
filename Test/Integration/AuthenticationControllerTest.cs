using System;
using System.Collections.Generic;
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
    public class AuthenticationControllerTest
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
        public async Task AnyoneLoggedIn_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = _factory.userAdmin;

            var response = await _client.GetAsync("api/authentication/anyoneloggedin");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task AnyoneLoggedIn_ShouldReturn_BadRequestIfError()
        {
            LoggedInAs = null;

            var response = await _client.GetAsync("api/authentication/anyoneloggedin");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task GetLoggedInUser_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = _factory.userAdmin;

            var response = await _client.GetAsync("api/authentication/loggedin");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task GetLoggedInUser_ShouldReturn_NotFoundIfError()
        {
            LoggedInAs = null;

            var response = await _client.GetAsync("api/authentication/loggedin");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task DoesUserHaveCorrectRights_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = _factory.userAdmin;

            var response = await _client.GetAsync("api/authentication/1");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task DoesUserHaveCorrectRights_ShouldReturn_BadRequestIfError()
        {
            LoggedInAs = _factory.userEmployee1;

            var response = await _client.GetAsync("api/authentication/1");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task IsUserAdminOrEmployee_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = _factory.userAdmin;

            var response = await _client.GetAsync("api/authentication/elevatedrights");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task IsUserAdminOrEmployee_ShouldReturn_BadRequestIfError()
        {
            LoggedInAs = _factory.userCustomer2;

            var response = await _client.GetAsync("api/authentication/elevatedrights");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task IsEmployee_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = _factory.userEmployee1;

            var response = await _client.GetAsync("api/authentication/employee");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task IsEmployee_ShouldReturn_BadRequestIfError()
        {
            LoggedInAs = _factory.userCustomer2;

            var response = await _client.GetAsync("api/authentication/employee");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task IsAdmin_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = _factory.userAdmin;

            var response = await _client.GetAsync("api/authentication/admin");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task IsAdmin_ShouldReturn_BadRequestIfError()
        {
            LoggedInAs = _factory.userCustomer2;

            var response = await _client.GetAsync("api/authentication/admin");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task LogoutUser_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = _factory.userAdmin;

            var response = await _client.GetAsync("api/authentication/logout");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task LogIn_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = null;

            var response = await _client.PostAsync("api/authentication/login",
                new StringContent(
                    JsonConvert.SerializeObject(new string[] { _factory.userAdmin.UserName, "aaaaaa" }),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task LogIn_ShouldReturn_NotFoundIfError()
        {
            LoggedInAs = null;

            var response = await _client.PostAsync("api/authentication/login",
                new StringContent(
                    JsonConvert.SerializeObject(new string[] { "sdsdsdsdsdsdsd", "aaaaaaaaaaaaaaaa" }),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }
    }
}
