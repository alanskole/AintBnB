using static AintBnB.BusinessLogic.Services.AuthenticationService;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AintBnB.WebApi.Controllers
{
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
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
            try
            {
                CorrectUser(id);
                return Ok("User can access");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/[controller]/admin")]
        public IActionResult IsUserAdmin()
        {
            try
            {
                AdminChecker();
                return Ok("User is admin");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
