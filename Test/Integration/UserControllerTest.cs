using AintBnB.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace Test.Integration
{
    [TestClass]
    public class UserControllerTest
    {
        private static CustomWebApplicationFactory _factory;
        private HttpClient _client;

        [TestInitialize]
        public void SetUp()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [TestCleanup]
        public void TearDown()
        {
            _factory.DisposeDb();
        }

        [TestMethod]
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
            LoggedInAs = _factory.userAdmin;

            var response = await _client.GetAsync("api/user/1");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetUser_ShouldReturn_NotFoundIfError()
        {
            LoggedInAs = _factory.userAdmin;

            var response = await _client.GetAsync("api/user/10000");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task UpdateUser_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = _factory.userAdmin;

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

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task UpdateUser_ShouldReturn_BadRequestIfError()
        {
            LoggedInAs = _factory.userAdmin;

            User usr = new User
            {
            };

            var response = await _client.PutAsync("api/user/6",
                new StringContent(
                    JsonConvert.SerializeObject(usr),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetAllUser_ShouldReturn_SuccessStatus()
        {

            LoggedInAs = _factory.userAdmin;

            var response = await _client.GetAsync("api/user");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetAllUser_ShouldReturn_NotFoundIfError()
        {

            LoggedInAs = _factory.userCustomer1;

            var response = await _client.GetAsync("api/user");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetAllCustomers_ShouldReturn_SuccessStatus()
        {

            LoggedInAs = _factory.userAdmin;

            var response = await _client.GetAsync("api/user/allcustomers");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task GetAllCustomers_ShouldReturn_NotFoundIfError()
        {

            LoggedInAs = _factory.userCustomer1;

            var response = await _client.GetAsync("api/user/allcustomers");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task DeleteUser_ShouldReturn_SuccessStatus()
        {
            LoggedInAs = _factory.userAdmin;

            var response = await _client.DeleteAsync("api/user/3");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task DeleteUser_ShouldReturn_BadRequestIfError()
        {
            LoggedInAs = _factory.userAdmin;

            var response = await _client.DeleteAsync("api/user/600");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
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

        [TestMethod]
        public async Task ChangePassword_ShouldReturn_BadRequestIfError()
        {
            LoggedInAs = _factory.userAdmin;

            var response = await _client.PostAsync("api/user/change",
                new StringContent(
                    JsonConvert.SerializeObject(new string[] { "aaaaaaaaaaaaaa", "1", "bbbbbb", "bbbbbb" }),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }
    }
}
