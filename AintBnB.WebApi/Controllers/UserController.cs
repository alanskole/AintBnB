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