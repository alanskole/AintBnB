using System.Net.Http;

namespace AintBnB.CommonMethodsAndProperties
{
    internal class HttpClientProvider
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
