using AintBnB.App.CommonMethodsAndProperties;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage;
using static Windows.Storage.ApplicationData;

namespace AintBnB.App.Helpers
{
    internal static class UwpCookieHelper
    {
        internal static ApplicationDataContainer localSettings = Current.LocalSettings;

        internal static async Task AddAuthCookieAsync(HttpClientHandler clientHandler)
        {
            if (localSettings.Values.ContainsKey("myCoockie"))
            {
                var cookieValueLocalSettings = await DecryptCookieValueAsync(localSettings.Values["myCoockie"].ToString());
                if (cookieValueLocalSettings != null)
                {
                    var cookie = new Cookie();
                    cookie.Name = "myCoockie";
                    cookie.Path = "/";
                    cookie.Domain = "localhost";
                    cookie.HttpOnly = true;
                    cookie.Secure = true;
                    cookie.Value = cookieValueLocalSettings.ToString();
                    clientHandler.CookieContainer.Add(cookie);
                }
            }
        }

        internal static async Task GetCsrfToken(HttpClientProvider clientProvider, string url = "https://localhost:44342/api/authentication/login")
        {
            await clientProvider.client.GetAsync(url);

            var responseCookies = clientProvider.clientHandler.CookieContainer.GetCookies(new Uri(url)).Cast<Cookie>();

            foreach (var cookiee in responseCookies)
            {
                if (!cookiee.Name.Equals("XSRF-TOKEN", StringComparison.OrdinalIgnoreCase))
                    clientProvider.clientHandler.CookieContainer.Add(cookiee);
            }

            try
            {
                var cookieCsrf = responseCookies.Single(o => o.Name == "XSRF-TOKEN").Value;
                clientProvider.client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", cookieCsrf);
            }
            catch
            {
            }
        }

        internal static async Task<string> EncryptCookieValueAsync(string message)
        {
            if (string.IsNullOrEmpty(message))
                return message;

            var base64message = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(message));
            var buffer = CryptographicBuffer.DecodeFromBase64String(base64message);
            var protectedData = new DataProtectionProvider("LOCAL=user");
            var encryptedBuffer = await protectedData.ProtectAsync(buffer);
            return CryptographicBuffer.EncodeToBase64String(encryptedBuffer);
        }

        internal static async Task<string> DecryptCookieValueAsync(string message)
        {
            if (string.IsNullOrEmpty(message))
                return message;

            var buffer = CryptographicBuffer.DecodeFromBase64String(message);
            var protectedData = new DataProtectionProvider("LOCAL=user");
            var decryptedBuffer = await protectedData.UnprotectAsync(buffer);
            var base64message = CryptographicBuffer.EncodeToBase64String(decryptedBuffer);
            var msgContents = Convert.FromBase64String(base64message);
            return System.Text.Encoding.UTF8.GetString(msgContents, 0, msgContents.Length);
        }
    }
}
