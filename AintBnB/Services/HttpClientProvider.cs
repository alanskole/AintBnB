using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AintBnB.Services
{
    public class HttpClientProvider
    {
        public string LocalHostAddress = "https://localhost:";
        public string LocalHostPort = "44342/";
        public string ControllerPartOfUri;
        public readonly HttpClientHandler clientHandler;
        public readonly HttpClient client;
        public HttpClientProvider()
        {
            clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            client = new HttpClient(clientHandler);
        }
    }
}
