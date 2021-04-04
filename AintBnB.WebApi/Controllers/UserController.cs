using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;

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
        public IActionResult CreateUser([FromBody] User user)
        {
            try
            {
                User newUser = _userService.CreateUser(user.UserName, user.Password, user.FirstName, user.LastName, user.UserType);
                return Created(HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + HttpContext.Request.Path + "/" + newUser.Id, newUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("api/[controller]/{id}")]
        public IActionResult UpdateUser([FromRoute] int id, [FromBody] User user)
        {
            try
            {
                _userService.UpdateUser(id, user);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("api/[controller]/change")]
        public IActionResult ChangePassword([FromBody] string[] elements)
        {
            try
            {
                _userService.ChangePassword(elements[0], Int32.Parse(elements[1]), elements[2], elements[3]);

                return Ok("Password change ok!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/[controller]")]
        public IActionResult GetAllUsers()
        {
            try
            {
                return Ok(_userService.GetAllUsers());
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/[controller]/allcustomers")]
        public IActionResult GetAllCustomers()
        {
            try
            {
                return Ok(_userService.GetAllUsersWithTypeCustomer());
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/[controller]/requests")]
        public IActionResult GetAllEmployeeRequests()
        {
            try
            {
                return Ok(_userService.GetAllEmployeeRequests());
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/[controller]/{id}")]
        public IActionResult GetUser([FromRoute] int id)
        {
            try
            {
                return Ok(_userService.GetUser(id));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete]
        [Route("api/[controller]/{id}")]
        public IActionResult DeleteUser([FromRoute] int id)
        {
            try
            {
                _deletionService.DeleteUser(id);
                return Ok("Deletion ok");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}