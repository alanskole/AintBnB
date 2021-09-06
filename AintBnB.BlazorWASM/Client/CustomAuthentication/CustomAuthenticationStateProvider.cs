using AintBnB.Core.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AintBnB.BlazorWASM.Client.CustomAuthentication
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private HttpClient _httpClient;

        public CustomAuthenticationStateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var response = await _httpClient.GetAsync(new Uri(_httpClient.BaseAddress + "authentication/currentUserIdAndRole"));

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var currentUser = JsonConvert.DeserializeObject<User>(json);

                var userClaims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, currentUser.Id.ToString()),
                    new Claim(ClaimTypes.Role, currentUser.UserType.ToString())
                };

                var claimsId = new ClaimsIdentity(userClaims, "User Identity");

                var userPrincipal = new ClaimsPrincipal(new[] { claimsId });

                return new AuthenticationState(userPrincipal);
            }
            else
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }
}
