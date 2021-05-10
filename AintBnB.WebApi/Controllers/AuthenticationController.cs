using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace AintBnB.WebApi.Controllers
{
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private IUserService _userService;

        public AuthenticationController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>API GET request that checks if anyone is logged in</summary>
        /// <returns>Status 200 if true, otherwise status code 400</returns>
        [HttpGet]
        [Route("api/[controller]/anyoneloggedin")]
        public IActionResult IsAnyoneLoggedIn()
        {
            try
            {
                AnyoneLoggedIn();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>API GET request to get the user that's logged in.</summary>
        /// <returns>Status 200 and the user if successful, otherwise status code 404</returns>
        [HttpGet]
        [Route("api/[controller]/loggedin")]
        public IActionResult GetLoggedInUser()
        {
            if (LoggedInAs == null)
                return NotFound("No one logged in");

            return Ok(LoggedInAs);
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

            if (CorrectUserOrAdminOrEmployee(user))
                return Ok("User can access");
            else
                return BadRequest("Restricted access!");
        }

        /// <summary>API GET request to check if user that's logged in is admin or employee</summary>
        /// <returns>Status 200  if true, otherwise status code 400</returns>
        [HttpGet]
        [Route("api/[controller]/elevatedrights")]
        public IActionResult IsUserAdminOrEmployee()
        {
            if (HasElevatedRights())
                return Ok("User can access");
            else
                return BadRequest("User is neither admin or employee!");
        }

        /// <summary>API GET request to check if the user that's logged in is employee</summary>
        /// <returns>Status 200 if true, otherwise status code 400</returns>
        [HttpGet]
        [Route("api/[controller]/employee")]
        public IActionResult IsEmployee()
        {
            if (EmployeeChecker())
                return Ok("User is employee");
            else
                return BadRequest("User is not employee!");
        }

        /// <summary>API GET request to check if the user that's logged in is admin</summary>
        /// <returns>Status 200 if true, otherwise status code 400</returns>
        [HttpGet]
        [Route("api/[controller]/admin")]
        public IActionResult IsUserAdmin()
        {
            if (AdminChecker())
                return Ok("User is admin");
            else
                return BadRequest("User is not admin!");
        }

        /// <summary>API GET request to logout the user</summary>
        /// <returns>Status 200 if successful, otherwise status code 400</returns>
        [HttpGet]
        [Route("api/[controller]/logout")]
        public IActionResult LogoutUser()
        {
            Logout();

            if (LoggedInAs == null)
                return Ok("Logout ok!");
            else
                return BadRequest("Failed to logout!");
        }


        /// <summary>API POST request that logs in a user.</summary>
        /// <param name="usernameAndPassword">An array with the username and password of the user that tries to log in.</param>
        /// <returns>Status 200 if successful, otherwise status code 404</returns>
        [HttpPost]
        [Route("api/[controller]/login")]
        public async Task<IActionResult> LogInAsync([FromBody] string[] usernameAndPassword)
        {
            try
            {
                TryToLogin(usernameAndPassword[0], usernameAndPassword[1], await _userService.GetAllUsersForLoginAsync());

                return Ok("Login ok!");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}