using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Test.Integration
{
    [TestClass]
    public class AuthenticationControllerTest
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
        public async Task GetLoggedInUser_ShouldReturn_SuccessStatus()
        {
            var response = await _client.GetAsync("api/authentication/loggedinUserId");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task DoesUserHaveCorrectRights_ShouldReturn_SuccessStatus()
        {
            var response = await _client.GetAsync("api/authentication/1");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task DoesUserHaveCorrectRights_ShouldReturn_BadRequestIfError()
        {
            _client = _factory.CreateClient();

            await _factory.LoginUserAsync(_client, new string[] { _factory.userCustomer1.UserName, "aaaaaa" });

            var response = await _client.GetAsync("api/authentication/3");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task IsAdmin_ShouldReturn_SuccessStatus()
        {
            var response = await _client.GetAsync("api/authentication/admin");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task IsAdmin_ShouldReturn_BadRequestIfError()
        {
            _client = _factory.CreateClient();

            await _factory.LoginUserAsync(_client, new string[] { _factory.userCustomer1.UserName, "aaaaaa" });
            
            var response = await _client.GetAsync("api/authentication/admin");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task LogoutUser_ShouldReturn_SuccessStatus()
        {
            var response = await _client.PostAsync("api/authentication/logout",
                new StringContent(
                    JsonConvert.SerializeObject(null),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task LogIn_ShouldReturn_SuccessStatus()
        {
            _client = _factory.CreateClient();

            var response = await _client.PostAsync("api/authentication/login",
                new StringContent(
                    JsonConvert.SerializeObject(new string[] { _factory.userAdmin.UserName, "aaaaaa" }),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task LogIn_ShouldReturn_NotFoundIfError()
        {
            _client = _factory.CreateClient();

            var response = await _client.PostAsync("api/authentication/login",
                new StringContent(
                    JsonConvert.SerializeObject(new string[] { "sdsdsdsdsdsdsd", "aaaaaaaaaaaaaaaa" }),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [TestMethod]
        public async Task LogIn_ShouldReturn_BadRequestIfAlreadyLoggedIn()
        {
            var response = await _client.PostAsync("api/authentication/login",
                new StringContent(
                    JsonConvert.SerializeObject(new string[] { _factory.userAdmin.UserName, "aaaaaa" }),
                    Encoding.UTF8,
                    "application/json"));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }
    }
}
