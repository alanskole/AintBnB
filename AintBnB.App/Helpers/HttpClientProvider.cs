using System;
using System.Net;
using System.Net.Http;

namespace AintBnB.App.Helpers
{
    internal class HttpClientProvider : IDisposable
    {
        public readonly HttpClientHandler clientHandler;
        public readonly HttpClient client;
        public HttpClientProvider()
        {
            clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            clientHandler.CookieContainer = new CookieContainer();
            client = new HttpClient(clientHandler) { BaseAddress = new Uri("https://localhost:44348/api/") };
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
