using AintBnB.Core.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AintBnB.WebApi.Helpers
{
    internal static class CurrentUserDetails
    {
        internal static UserTypes GetUsertypeOfLoggedInUser(HttpContext httpContext)
        {
            Enum.TryParse(httpContext.User.FindFirst(ClaimTypes.Role)?.Value, out UserTypes userType);

            return userType;
        }

        internal static int GetIdOfLoggedInUser(HttpContext httpContext)
        {
            return Int32.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    }
}
