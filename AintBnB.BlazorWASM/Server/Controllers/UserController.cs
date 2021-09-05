using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static AintBnB.BusinessLogic.Helpers.Authentication;
using static AintBnB.BlazorWASM.Server.Helpers.CurrentUserDetails;

namespace AintBnB.BlazorWASM.Server.Controllers
{
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private IUserService _userService;
        private IDeletionService _deletionService;

        public UserController(IUserService userService, IDeletionService deletionService)
        {
            _userService = userService;
            _deletionService = deletionService;
        }

        /// <summary>API POST request to create a user.</summary>
        /// <param name="user">The user to create.</param>
        /// <returns>Status code 201 if successful, otherwise status 400</returns>
        [HttpPost]
        [Route("api/[controller]")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUserAsync([FromBody] User user)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
                return BadRequest("You're already logged in with a user account!");

            try
            {
                var newUser = await _userService.CreateUserAsync(user.UserName, user.Password, user.FirstName, user.LastName, user.UserType);
                return Created(HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + HttpContext.Request.Path + "/" + newUser.Id, newUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>API PUT request to update a user.</summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="user">The updated user object.</param>
        /// <returns>Status 200 and the updated user if successful, otherwise status code 400</returns>
        [HttpPut]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> UpdateUserAsync([FromRoute] int id, [FromBody] User user)
        {
            try
            {
                var old = await _userService.GetUserAsync(id);

                if (!CorrectUserOrAdmin(old.Id, GetIdOfLoggedInUser(HttpContext), GetUsertypeOfLoggedInUser(HttpContext)))
                    return BadRequest(new AccessException().Message);

                await _userService.UpdateUserAsync(id, user, GetUsertypeOfLoggedInUser(HttpContext));
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>API POST request to change password of a user.</summary>
        /// <param name="elements">An array containing the original password, the user-ID of the user to change the password of, the new password and a confirmation of the new password.</param>
        /// <returns>Status 200 if successful, otherwise status code 400</returns>
        [HttpPost]
        [Route("api/[controller]/change")]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] string[] elements)
        {
            try
            {
                if (GetIdOfLoggedInUser(HttpContext) != int.Parse(elements[1]))
                    return BadRequest(new AccessException("Only the owner of the account can change their password!").Message);

                await _userService.ChangePasswordAsync(elements[0], int.Parse(elements[1]), elements[2], elements[3]);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>API GET request that gets all users.</summary>
        /// <returns>Status 200 and all the users if successful, otherwise status code 404</returns>
        [HttpGet]
        [Route("api/[controller]")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            try
            {
                if (!AdminChecker(GetUsertypeOfLoggedInUser(HttpContext)))
                    return NotFound(new AccessException().Message);

                return Ok(await _userService.GetAllUsersAsync());
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API GET request that gets all users with usertype customer.</summary>
        /// <returns>Status 200 and all the users with usertype customer if successful, otherwise status code 404</returns>
        [HttpGet]
        [Route("api/[controller]/allcustomers")]
        public async Task<IActionResult> GetAllCustomersAsync()
        {
            try
            {
                if (!AdminChecker(GetUsertypeOfLoggedInUser(HttpContext)))
                    return NotFound(new AccessException().Message);

                return Ok(await _userService.GetAllUsersWithTypeCustomerAsync());
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API GET request to fetch a user from the database.</summary>
        /// <param name="id">The ID of the user to get.</param>
        /// <returns>Status 200 and the requested user if successful, otherwise status code 404</returns>
        [HttpGet]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> GetUserAsync([FromRoute] int id)
        {
            try
            {
                var user = await _userService.GetUserAsync(id);

                if (!CorrectUserOrAdmin(user.Id, GetIdOfLoggedInUser(HttpContext), GetUsertypeOfLoggedInUser(HttpContext)))
                    return NotFound(new AccessException().Message);

                return Ok(user);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API DELETE request to delete a user from the database.</summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>Status 200 if successful, otherwise status code 400</returns>
        [HttpDelete]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> DeleteUserAsync([FromRoute] int id)
        {
            try
            {
                var user = await _userService.GetUserAsync(id);

                if (!CorrectUserOrAdmin(user.Id, GetIdOfLoggedInUser(HttpContext), GetUsertypeOfLoggedInUser(HttpContext)))
                    return BadRequest(new AccessException($"Administrator or user with ID {id} only!").Message);

                await _deletionService.DeleteUserAsync(id);

                if (!AdminChecker(GetUsertypeOfLoggedInUser(HttpContext)))
                    await HttpContext.SignOutAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}