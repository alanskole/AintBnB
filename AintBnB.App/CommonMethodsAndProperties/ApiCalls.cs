using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AintBnB.App.CommonMethodsAndProperties
{
    internal static class ApiCalls
    {

        /// <summary>Makes a API POST call to create a new object.</summary>
        /// <param name="uri">The URI of the API call.</param>
        /// <param name="objJson">The object that will be created.</param>
        /// <param name="clientProvider">The client provider containing the HttpClient that will be used to make the API call.</param>
        public static async Task PostAsync(string uri, object objJson, HttpClientProvider clientProvider)
        {
            var json = JsonConvert.SerializeObject(objJson);
            var response = await clientProvider.client.PostAsync(
                new Uri(uri), new StringContent(json, Encoding.UTF8, "application/json"));
            ResponseChecker(response);
        }

        /// <summary>Makes a API GET call to get an object.</summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="uri">The URI of the API call.</param>
        /// <param name="clientProvider">The client provider containing the HttpClient that will be used to make the API call.</param>
        /// <returns>The object that was requested</returns>
        public static async Task<T> GetAsync<T>(string uri, HttpClientProvider clientProvider)
        {
            var response = await clientProvider.client.GetAsync(new Uri(uri));
            ResponseChecker(response);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>Makes a API GET call.</summary>
        /// <param name="uri">The URI of the API call.</param>
        /// <param name="clientProvider">The client provider containing the HttpClient that will be used to make the API call.</param>
        public static async Task GetAsync(string uri, HttpClientProvider clientProvider)
        {
            var response = await clientProvider.client.GetAsync(new Uri(uri));
            ResponseChecker(response);
        }

        /// <summary>Makes a API GET call to get a list of objects.</summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="uri">The URI of the API call.</param>
        /// <param name="clientProvider">The client provider containing the HttpClient that will be used to make the API call.</param>
        /// <returns>A list of the objects that was requested</returns>
        public static async Task<List<T>> GetAllAsync<T>(string uri, HttpClientProvider clientProvider)
        {
            var response = await clientProvider.client.GetAsync(new Uri(uri));
            ResponseChecker(response);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<T>>(json);
        }

        /// <summary>Makes a API PUT call to update an existing object.</summary>
        /// <param name="uri">The URI of the API call.</param>
        /// <param name="objJson">The object that will be updated.</param>
        /// <param name="clientProvider">The client provider containing the HttpClient that will be used to make the API call.</param>
        public static async Task PutAsync(string uri, object objJson, HttpClientProvider clientProvider)
        {
            var json = JsonConvert.SerializeObject(objJson);

            var response = await clientProvider.client.PutAsync(
                new Uri(uri), new StringContent(json, Encoding.UTF8, "application/json"));
            ResponseChecker(response);
        }

        /// <summary>Makes a API DELETE call to delete an object.</summary>
        /// <param name="uri">The URI of the API call.</param>
        /// <param name="clientProvider">The client provider containing the HttpClient that will be used to make the API call.</param>
        public static async Task DeleteAsync(string uri, HttpClientProvider clientProvider)
        {
            var response = await clientProvider.client.DeleteAsync(new Uri(uri));
            ResponseChecker(response);
        }

        public static async Task<List<T>> SortListAsync<T>(string uri, List<T> listJson, HttpClientProvider clientProvider)
        {
            var json = JsonConvert.SerializeObject(listJson);
            var response = await clientProvider.client.PostAsync(
                new Uri(uri), new StringContent(json, Encoding.UTF8, "application/json"));
            ResponseChecker(response);
            return JsonConvert.DeserializeObject<List<T>>(await response.Content.ReadAsStringAsync());
        }

        /// <summary>Checks the httpresponse of a HttpClient call for errors</summary>
        /// <param name="response">The response to check.</param>
        /// <exception cref="ArgumentException">If the repsonse doesn't contain a success code</exception>
        public static void ResponseChecker(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }
    }
}
