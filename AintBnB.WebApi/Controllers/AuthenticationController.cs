using static AintBnB.BusinessLogic.Services.AuthenticationService;
using Microsoft.AspNetCore.Mvc;
using System;
using AintBnB.BusinessLogic.CustomExceptions;

namespace AintBnB.WebApi.Controllers
{
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
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
            if (CorrectUserOrAdminOrEmployee(id))
                return Ok("User can access");
            else
                return BadRequest(new AccessException());
        }

        [HttpGet]
        [Route("api/[controller]/elevatedrights")]
        public IActionResult IsUserAdminOrEmployee([FromRoute] int id)
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


        [HttpGet]
        [Route("api/[controller]/login/{check}")]
        public IActionResult LogIn([FromRoute] string check)
        {
            try
            {
                string[] words = check.Split(' ');

                TryToLogin(words[0], words[1]);

                return Ok("Login ok!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
