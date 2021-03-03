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
    public class UserControllerTest
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
        public async Task CreateUser_ShouldReturn_SuccessStatus()
        {
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

        [Test]
        public async Task GetUser_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = _factory.userAdmin;
            
            var response = await _client.GetAsync("api/user/1");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task UpdateUser_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = _factory.userAdmin;

            User usr = new User
            {
                FirstName = "dd",
                LastName = "ff",
                UserType = UserTypes.Customer
            };

            var response = await _client.PutAsync("api/user/6",
                new StringContent(
                    JsonConvert.SerializeObject(usr),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task GetAllUser_ShouldReturn_SuccessStatus()
        {

            LoggedInAs = _factory.userAdmin;

            var response = await _client.GetAsync("api/user");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task GetAllCustomers_ShouldReturn_SuccessStatus()
        {

            LoggedInAs = _factory.userEmployee1;

            var response = await _client.GetAsync("api/user/allcustomers");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task GetAllRequestsToBecomeEmployee_ShouldReturn_SuccessStatus()
        {

            LoggedInAs = _factory.userAdmin;

            var response = await _client.GetAsync("api/user/requests");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task DeleteUser_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = _factory.userAdmin;

            var response = await _client.DeleteAsync("api/user/6");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Test]
        public async Task ChangePassword_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = _factory.userAdmin;

            var response = await _client.PostAsync("api/user/change",
                new StringContent(
                    JsonConvert.SerializeObject(new string[] { "aaaaaa", "1", "bbbbbb", "bbbbbb" }),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }
    }
}
