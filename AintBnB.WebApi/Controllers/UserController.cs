using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AintBnB.WebApi.Controllers
{
    [ApiController]
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
        public async Task<IActionResult> CreateUserAsync([FromBody] User user)
        {
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
                await _userService.UpdateUserAsync(id, user);
                return Ok(user);
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
                await _userService.ChangePasswordAsync(elements[0], Int32.Parse(elements[1]), elements[2], elements[3]);

                return Ok("Password change ok!");
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
                return Ok(await _userService.GetAllUsersWithTypeCustomerAsync());
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API GET request that gets all users with usertype employeerequest.</summary>
        /// <returns>Status 200 and all the users with usertype employeerequest if successful, otherwise status code 404</returns>
        [HttpGet]
        [Route("api/[controller]/requests")]
        public async Task<IActionResult> GetAllEmployeeRequestsAsync()
        {
            try
            {
                return Ok(await _userService.GetAllEmployeeRequestsAsync());
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
                return Ok(await _userService.GetUserAsync(id));
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
                await _deletionService.DeleteUserAsync(id);
                return Ok("Deletion ok");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}