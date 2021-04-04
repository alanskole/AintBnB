using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;
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
        public IActionResult DoesUserHaveCorrectRights([FromRoute] int id)
        {
            User user;
            try
            {
                user = _userService.GetUser(id);
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

        [HttpGet]
        [Route("api/[controller]/elevatedrights")]
        public IActionResult IsUserAdminOrEmployee()
        {
            if (HasElevatedRights())
                return Ok("User can access");
            else
                return BadRequest("User is neither admin or employee!");
        }

        [HttpGet]
        [Route("api/[controller]/employee")]
        public IActionResult IsEmployee()
        {
            if (EmployeeChecker())
                return Ok("User is employee");
            else
                return BadRequest("User is not employee!");
        }

        [HttpGet]
        [Route("api/[controller]/admin")]
        public IActionResult IsUserAdmin()
        {
            if (AdminChecker())
                return Ok("User is admin");
            else
                return BadRequest("User is not admin!");
        }

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


        [HttpPost]
        [Route("api/[controller]/login")]
        public IActionResult LogIn([FromBody] string[] usernameAndPassword)
        {
            try
            {
                TryToLogin(usernameAndPassword[0], usernameAndPassword[1], _userService.GetAllUsersForLogin());

                return Ok("Login ok!");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}