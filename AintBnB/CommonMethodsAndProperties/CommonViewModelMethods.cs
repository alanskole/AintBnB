using System;
using System.Net.Http;

namespace AintBnB.CommonMethodsAndProperties
{
    public static class CommonViewModelMethods
    {
        public static void ResponseChecker(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }
    }
}
