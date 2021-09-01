using System;
using System.Net;
using System.Net.Http;

namespace AintBnB.App.CommonMethodsAndProperties
{
    internal class HttpClientProvider : IDisposable
    {
        public string LocalHostAddress = "https://localhost:";
        public string LocalHostPort = "44342/";
        public readonly HttpClientHandler clientHandler;
        public readonly HttpClient client;
        public HttpClientProvider()
        {
            clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            clientHandler.CookieContainer = new CookieContainer();
            client = new HttpClient(clientHandler);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
