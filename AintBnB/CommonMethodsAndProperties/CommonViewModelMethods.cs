using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AintBnB.CommonMethodsAndProperties
{
    internal static class CommonViewModelMethods
    {
        public static async Task PostAsync(string uri, object objJson, HttpClientProvider clientProvider)
        {
            var json = JsonConvert.SerializeObject(objJson);
            var response = await clientProvider.client.PostAsync(
                new Uri(uri), new StringContent(json, Encoding.UTF8, "application/json"));
            ResponseChecker(response);
        }

        public static async Task<T> GetAsync<T>(string uri, HttpClientProvider clientProvider)
        {
            var response = await clientProvider.client.GetAsync(new Uri(uri));
            ResponseChecker(response);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static async Task GetAsync(string uri, HttpClientProvider clientProvider)
        {
            var response = await clientProvider.client.GetAsync(new Uri(uri));
            ResponseChecker(response);
        }

        public static async Task<List<T>> GetAllAsync<T>(string uri, HttpClientProvider clientProvider)
        {
            var response = await clientProvider.client.GetAsync(new Uri(uri));
            ResponseChecker(response);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<T>>(json);
        }

        public static async Task PutAsync(string uri, object objJson, HttpClientProvider clientProvider)
        {
            var json = JsonConvert.SerializeObject(objJson);

            var response = await clientProvider.client.PutAsync(
                new Uri(uri), new StringContent(json, Encoding.UTF8, "application/json"));
            ResponseChecker(response);
        }

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

        public static void ResponseChecker(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }
    }
}
