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

namespace Test.WebApi
{
    [TestFixture]
    public class UserControllerTest
    {
        private static CustomWebApplicationFactory _factory;
        private HttpClient _client;

        [SetUp]
        public void ClassInit()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [TearDown]
        public void ClassCleanup()
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
            var result = JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
            Assert.AreEqual(7, result.Id);
            Assert.AreEqual(usr.UserName, result.UserName);
            Assert.AreEqual(usr.FirstName, result.FirstName);
            Assert.AreEqual(usr.LastName, result.LastName);
            Assert.AreEqual(UserTypes.Customer, result.UserType);
        }

        [Test]
        public async Task GetUser_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = _factory.userAdmin;
            
            var response = await _client.GetAsync("api/user/1");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
            var result = JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("admin", result.UserName);
            Assert.AreEqual("Ad", result.FirstName);
            Assert.AreEqual("Min", result.LastName);
            Assert.AreEqual(UserTypes.Admin, result.UserType);
        }

        [Test]
        public async Task GetAllUser_ShouldReturn_SuccessStatus()
        {

            LoggedInAs = _factory.userAdmin;

            var response = await _client.GetAsync("api/user");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
            var result = JsonConvert.DeserializeObject<List<User>>(await response.Content.ReadAsStringAsync());
            Assert.AreEqual(6, result.Count);
            Assert.AreEqual(UserTypes.Admin, result[0].UserType);
            Assert.AreEqual(UserTypes.Customer, result[5].UserType);
        }
    }
}
