using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using static AintBnB.BusinessLogic.Helpers.Authentication;
using static AintBnB.BlazorWASM.Server.Helpers.CurrentUserDetails;

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

        /// <summary>API GET request to get the user that's logged in.</summary>
        /// <returns>Status 200 and the user if successful, otherwise status code 404</returns>
        [HttpGet]
        [Route("api/[controller]/isLoggedIn")]
        [AllowAnonymous]
        public IActionResult IsLoggedIn()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
                return Ok();
            else
                return NotFound();
        }

        /// <summary>API GET request to get the user that's logged in.</summary>
        /// <returns>Status 200 and the user if successful, otherwise status code 404</returns>
        [HttpGet]
        [Route("api/[controller]/loggedinUserId")]
        public IActionResult GetLoggedInUserId()
        {
            var id = GetIdOfLoggedInUser(HttpContext);
            return Ok(id);
        }

        [HttpGet]
        [Route("api/[controller]/currentUserIdAndRole")]
        [AllowAnonymous]
        public IActionResult GetLoggedInUserIdAndRole()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                var id = GetIdOfLoggedInUser(HttpContext);
                var userType = GetUsertypeOfLoggedInUser(HttpContext);
                return Ok(new User { Id = id, UserType = userType });
            }
            else
                return NotFound();
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
                return BadRequest(ex.Message);
            }

            if (CorrectUserOrAdmin(user.Id, GetIdOfLoggedInUser(HttpContext), GetUsertypeOfLoggedInUser(HttpContext)))
                return Ok();
            else
                return BadRequest("Restricted access!");
        }

        /// <summary>API GET request to check if the user that's logged in is admin</summary>
        /// <returns>Status 200 if true, otherwise status code 400</returns>
        [HttpGet]
        [Route("api/[controller]/admin")]
        public IActionResult IsUserAdmin()
        {
            var userType = GetUsertypeOfLoggedInUser(HttpContext);

            if (AdminChecker(userType))
                return Ok();
            else
                return BadRequest("User is not admin!");
        }

        /// <summary>API POST request to logout the user</summary>
        /// <returns>Status 200 if successful, otherwise status code 400</returns>
        [HttpPost]
        [Route("api/[controller]/logout")]
        public async Task<IActionResult> LogoutUserAsync()
        {
            await HttpContext.SignOutAsync();
            return Ok();
        }


        /// <summary>API POST request that logs in a user.</summary>
        /// <param name="usernameAndPassword">An array with the username and password of the user that tries to log in.</param>
        /// <returns>Status 200 if successful, otherwise status code 404</returns>
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
                    new Claim(ClaimTypes.NameIdentifier, idAndUsertypeOfUserLoggingIn.Item1.ToString()),
                    new Claim(ClaimTypes.Role, idAndUsertypeOfUserLoggingIn.Item2.ToString())
                };

                var claimsId = new ClaimsIdentity(userClaims, "User Identity");

                var userPrincipal = new ClaimsPrincipal(new[] { claimsId });
                await HttpContext.SignInAsync(userPrincipal);

                return Ok();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}