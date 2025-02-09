﻿using AintBnB.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Test.Integration
{
    [TestClass]
    public class UserControllerTest
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
        public async Task CreateUser_ShouldReturn_SuccessStatus()
        {
            _client = _factory.CreateClient();

            User usr = new User
            {
                UserName = "nbb",
                Password = "aaaaaa",
                FirstName = "dd",
                LastName = "ff",
                UserType = UserTypes.Customer
            };
            var response = await _client.PostAsync("api/user",
                new StringContent(
                    JsonConvert.SerializeObject(usr),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task CreateUser_ShouldReturn_BadRequestIfAlreadyLoggedIn()
        {
            _client = _factory.CreateClient();

            User usr = new User
            {
            };
            var response = await _client.PostAsync("api/user",
                new StringContent(
                    JsonConvert.SerializeObject(usr),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task CreateUser_ShouldReturn_BadRequestIfError()
        {
            User usr = new User
            {
            };
            var response = await _client.PostAsync("api/user",
                new StringContent(
                    JsonConvert.SerializeObject(usr),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetUser_ShouldReturn_SuccessStatus()
        {
            var response = await _client.GetAsync("api/user/1");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetUser_ShouldReturn_BadRequestIfErrorWithAuthorization()
        {
            _client = _factory.CreateClient();
            await _factory.LoginUserAsync(_client, new string[] { _factory.userCustomer1.UserName, "aaaaaa" });

            var response = await _client.GetAsync($"api/user/{_factory.userCustomer2.Id}");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetUser_ShouldReturn_NotFoundIfErrorWithFecthingObject()
        {
            var response = await _client.GetAsync("api/user/10000");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task UpdateUser_ShouldReturn_SuccessStatus()
        {
            User usr = new User
            {
                FirstName = "dd",
                LastName = "ff",
                UserType = UserTypes.Customer
            };

            var response = await _client.PutAsync("api/user/3",
                new StringContent(
                    JsonConvert.SerializeObject(usr),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task UpdateUser_ShouldReturn_BadRequestIfErrorWithAuthorization()
        {
            _client = _factory.CreateClient();
            await _factory.LoginUserAsync(_client, new string[] { _factory.userCustomer1.UserName, "aaaaaa" });

            User usr = new User
            {
                FirstName = "dd",
                LastName = "ff",
                UserType = UserTypes.Customer
            };

            var response = await _client.PutAsync($"api/user/{_factory.userCustomer2.Id}",
                new StringContent(
                    JsonConvert.SerializeObject(usr),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task UpdateUser_ShouldReturn_BadRequestIfError()
        {
            User usr = new User
            {
            };

            var response = await _client.PutAsync("api/user/3",
                new StringContent(
                    JsonConvert.SerializeObject(usr),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task UpdateUser_ShouldReturn_NotFoundIfErrorWithFecthingObject()
        {
            User usr = new User
            {
            };

            var response = await _client.PutAsync("api/user/6",
                new StringContent(
                    JsonConvert.SerializeObject(usr),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetAllUsers_ShouldReturn_SuccessStatus()
        {
            var response = await _client.GetAsync("api/user");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetAllUsers_ShouldReturn_BadRequestIfErrorWithAuthorization()
        {
            _client = _factory.CreateClient();
            await _factory.LoginUserAsync(_client, new string[] { _factory.userCustomer1.UserName, "aaaaaa" });

            var response = await _client.GetAsync("api/user");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetAllCustomers_ShouldReturn_SuccessStatus()
        {
            var response = await _client.GetAsync("api/user/allcustomers");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetAllCustomers_ShouldReturn_BadRequestIfErrorWithAuthorization()
        {
            _client = _factory.CreateClient();

            await _factory.LoginUserAsync(_client, new string[] { _factory.userCustomer1.UserName, "aaaaaa" });

            var response = await _client.GetAsync("api/user/allcustomers");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task DeleteUser_ShouldReturn_SuccessStatus()
        {
            var response = await _client.DeleteAsync("api/user/2");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteUser_ShouldReturn_BadRequestIfErrorWithAuthorization()
        {
            _client = _factory.CreateClient();
            await _factory.LoginUserAsync(_client, new string[] { _factory.userCustomer1.UserName, "aaaaaa" });

            var response = await _client.DeleteAsync($"api/user/{_factory.userCustomer2.Id}");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task DeleteUser_ShouldReturn_BadRequestIfError()
        {
            var response = await _client.DeleteAsync($"api/user/{_factory.booking4.BookedBy.Id}");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task DeleteUser_ShouldReturn_NotFoundIfErrorWithFecthingObject()
        {
            var response = await _client.DeleteAsync("api/user/600");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task ChangePassword_ShouldReturn_SuccessStatus()
        {
            var response = await _client.PutAsync("api/user/change",
                new StringContent(
                    JsonConvert.SerializeObject(new string[] { "aaaaaa", "1", "bbbbbb", "bbbbbb" }),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task ChangePassword_ShouldReturn_BadRequestIfError()
        {
            var response = await _client.PutAsync("api/user/change",
                new StringContent(
                    JsonConvert.SerializeObject(new string[] { "aaaaaaaaaaaaaa", "1", "bbbbbb", "bbbbbb" }),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task ChangePassword_ShouldReturn_BadRequestIfErrorWithAuthorization()
        {
            var response = await _client.PutAsync("api/user/change",
                new StringContent(
                    JsonConvert.SerializeObject(new string[] { "aaaaaa", _factory.userCustomer2.Id.ToString(), "bbbbbb", "bbbbbb" }),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }
    }
}
