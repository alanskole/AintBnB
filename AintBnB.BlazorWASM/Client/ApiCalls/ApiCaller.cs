using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AintBnB.BlazorWASM.Client.ApiCalls
{
    internal class ApiCaller
    {
        internal HttpClient _httpClient;

        public ApiCaller(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>Makes a API POST call to create a new object.</summary>
        /// <param name="uri">The URI of the API call.</param>
        /// <param name="objJson">The object that will be created.</param>
        /// <param name="_httpClient">_httpClient that will be used to make the API call.</param>
        public async Task PostAsync(string uri, object objJson, string csrfToken)
        {
            _httpClient.DefaultRequestHeaders.Add("X-XSRF-TOKEN", csrfToken);

            var json = JsonConvert.SerializeObject(objJson);
            var response = await _httpClient.PostAsync(
                new Uri(_httpClient.BaseAddress + uri), new StringContent(json, Encoding.UTF8, "application/json"));
            ResponseChecker(response);
        }

        /// <summary>Makes a API GET call to get an object.</summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="uri">The URI of the API call.</param>
        /// <returns>The object that was requested</returns>
        public async Task<T> GetAsync<T>(string uri)
        {
            var response = await _httpClient.GetAsync(new Uri(_httpClient.BaseAddress + uri));
            ResponseChecker(response);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>Makes a API GET call.</summary>
        /// <param name="uri">The URI of the API call.</param>
        public async Task GetAsync(string uri)
        {
            var response = await _httpClient.GetAsync(new Uri(_httpClient.BaseAddress + uri));
            ResponseChecker(response);
        }

        /// <summary>Makes a API GET call to get a list of objects.</summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="uri">The URI of the API call.</param>
        /// <returns>A list of the objects that was requested</returns>
        public async Task<List<T>> GetAllAsync<T>(string uri)
        {
            var response = await _httpClient.GetAsync(new Uri(_httpClient.BaseAddress + uri));
            ResponseChecker(response);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<T>>(json);
        }

        /// <summary>Makes a API PUT call to update an existing object.</summary>
        /// <param name="uri">The URI of the API call.</param>
        /// <param name="objJson">The object that will be updated.</param>
        public async Task PutAsync(string uri, object objJson, string csrfToken)
        {
            _httpClient.DefaultRequestHeaders.Add("X-XSRF-TOKEN", csrfToken);

            var json = JsonConvert.SerializeObject(objJson);

            var response = await _httpClient.PutAsync(
                new Uri(_httpClient.BaseAddress + uri), new StringContent(json, Encoding.UTF8, "application/json"));
            ResponseChecker(response);
        }

        /// <summary>Makes a API DELETE call to delete an object.</summary>
        /// <param name="uri">The URI of the API call.</param>
        public async Task DeleteAsync(string uri, string csrfToken)
        {
            _httpClient.DefaultRequestHeaders.Add("X-XSRF-TOKEN", csrfToken);

            var response = await _httpClient.DeleteAsync(new Uri(_httpClient.BaseAddress + uri));
            ResponseChecker(response);
        }

        public async Task<List<T>> SortListAsync<T>(string uri, List<T> listJson)
        {
            var json = JsonConvert.SerializeObject(listJson);
            var response = await _httpClient.PostAsync(
                new Uri(_httpClient.BaseAddress + uri), new StringContent(json, Encoding.UTF8, "application/json"));
            ResponseChecker(response);
            return JsonConvert.DeserializeObject<List<T>>(await response.Content.ReadAsStringAsync());
        }

        /// <summary>Checks the httpresponse of a _httpClient call for errors</summary>
        /// <param name="response">The response to check.</param>
        /// <exception cref="ArgumentException">If the repsonse doesn't contain a success code</exception>
        public void ResponseChecker(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                _httpClient = new HttpClient { BaseAddress = _httpClient.BaseAddress };

                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
            }
        }
    }
}