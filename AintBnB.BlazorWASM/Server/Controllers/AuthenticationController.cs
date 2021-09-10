using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using static AintBnB.BlazorWASM.Server.Helpers.CurrentUserDetails;
using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace AintBnB.BlazorWASM.Server.Controllers
{
    [ApiController]
    [Authorize]
    public class AuthenticationController : ControllerBase
    {
        private IUserService _userService;

        public AuthenticationController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>API GET request to get the id and the usertype of the user that's logged in.</summary>
        /// <returns>Status 200 and a user object with the id and usertype of the logged in user, otherwise status 400</returns>
        [HttpGet]
        [Route("api/[controller]/currentUserIdAndRole")]
        [AllowAnonymous]
        public ActionResult<User> GetLoggedInUserIdAndRole()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                var id = GetIdOfLoggedInUser(HttpContext);
                var userType = GetUsertypeOfLoggedInUser(HttpContext);
                return new User { Id = id, UserType = userType };
            }
            else
                return BadRequest();
        }

        [HttpGet]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> DoesUserHaveCorrectRightsAsync([FromRoute] int id)
        {
            User user;
            try
            {
                user = await _userService.GetUserAsync(id);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            if (CorrectUserOrAdmin(user.Id, GetIdOfLoggedInUser(HttpContext), GetUsertypeOfLoggedInUser(HttpContext)))
                return NoContent();
            else
                return BadRequest("Restricted access!");
        }

        /// <summary>API POST request to logout the user</summary>
        /// <returns>Status 204 if successful</returns>
        [HttpPost]
        [Route("api/[controller]/logout")]
        public async Task<IActionResult> LogoutUserAsync()
        {
            await HttpContext.SignOutAsync();
            return NoContent();
        }


        /// <summary>API POST request that logs in a user.</summary>
        /// <param name="usernameAndPassword">An array with the username and password of the user that tries to log in.</param>
        /// <returns>Status 204 if successful, otherwise status code 400</returns>
        [HttpPost]
        [Route("api/[controller]/login")]
        [AllowAnonymous]
        public async Task<IActionResult> LogInAsync([FromBody] string[] usernameAndPassword)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
                await HttpContext.SignOutAsync();

            try
            {
                var idAndUsertypeOfUserLoggingIn = TryToLogin(usernameAndPassword[0], usernameAndPassword[1], await _userService.GetAllUsersAsync());

                var userClaims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, idAndUsertypeOfUserLoggingIn.Item1.ToString()),
                    new Claim(ClaimTypes.Role, idAndUsertypeOfUserLoggingIn.Item2.ToString())
                };

                var claimsId = new ClaimsIdentity(userClaims, "User Identity");

                var userPrincipal = new ClaimsPrincipal(new[] { claimsId });

                await HttpContext.SignInAsync(
                    userPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true
                    });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}