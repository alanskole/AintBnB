using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AintBnB.WebApi.Controllers
{
    [ApiController]
    public class AccommodationController : ControllerBase
    {
        private IAccommodationService _accommodationService;
        private IUserService _userService;
        private IDeletionService _deletionService;

        public AccommodationController(IAccommodationService accommodationService, IUserService userService, IDeletionService deletionService)
        {
            _accommodationService = accommodationService;
            _userService = userService;
            _deletionService = deletionService;
        }

        /// <summary>API POST request to create a accommodation</summary>
        /// <param name="days">The amount of days to create the schedule of the accommodation for.</param>
        /// <param name="userId">The user-ID of the owner of the accommodation.</param>
        /// <param name="accommodation">The accommodation object</param>
        /// <returns>Status 201 if successful, otherwise status code 400</returns>
        [HttpPost]
        [Route("api/[controller]/{days}/{userId}")]
        public async Task<IActionResult> CreateAccommodationAsync([FromRoute] int days, [FromRoute] int userId, [FromBody] Accommodation accommodation)
        {
            try
            {
                var owner = await _userService.GetUserAsync(userId);
                var newAccommodation = await _accommodationService.CreateAccommodationAsync(owner, accommodation.Address, accommodation.SquareMeters, accommodation.AmountOfBedrooms, accommodation.KilometersFromCenter, accommodation.Description, accommodation.PricePerNight, accommodation.CancellationDeadlineInDays, accommodation.Picture, days);
                return Created(HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + HttpContext.Request.Path + "/" + accommodation.Id, accommodation);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>API GET request to get all the available accommodations matching the search criterias</summary>
        /// <param name="city">The city that the accommodations must be located in.</param>
        /// <param name="country">The country that the accommodations must be located in.</param>
        /// <param name="startdate">The startdate to book from</param>
        /// <param name="nights">The amount of nights to book for</param>
        /// <returns>Status 200 and all the accommodations if successful, otherwise status code 404</returns>
        [HttpGet]
        [Route("api/[controller]/{country}/{city}/{startdate}/{nights}")]
        public async Task<IActionResult> FindAvailableAsync([FromRoute] string city, [FromRoute] string country, [FromRoute] string startdate, [FromRoute] int nights)
        {
            try
            {
                return Ok(await _accommodationService.FindAvailableAsync(country, city, startdate, nights));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API POST request to sort a list of accommodations in a specific order</summary>
        /// <param name="available">The list of accommodations that must be sorted.</param>
        /// <param name="sortBy">The order to sort by: Price, Distance, Rating or Size.</param>
        /// <param name="ascOrDesc">Sort in ascending or descending order</param>
        /// <returns>Status 200 and the correct sorting order if successful, otherwise status code 404</returns>
        [HttpPost]
        [Route("api/[controller]/sort/{sortBy}/{ascOrDesc}")]
        public IActionResult SortAvailableList([FromBody] List<Accommodation> available, [FromRoute] string sortBy, [FromRoute] string ascOrDesc)
        {
            try
            {
                return Ok(_accommodationService.SortListOfAccommodations(available, sortBy, ascOrDesc));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API GET request to expand the schedule of an accommodation by x amount of days</summary>
        /// <param name="id">The ID of the accommodation to expand the schedule of.</param>
        /// <param name="days">The amount of days to expand the schedule by.</param>
        /// <returns>Status 200 and the accommodation if successful, otherwise status code 404</returns>
        [HttpGet]
        [Route("api/[controller]/{id}/{days}")]
        public async Task<IActionResult> ExpandScheduleAsync([FromRoute] int id, [FromRoute] int days)
        {
            try
            {
                await _accommodationService.ExpandScheduleOfAccommodationWithXAmountOfDaysAsync(id, days);
                return Ok(await _accommodationService.GetAccommodationAsync(id));
            }
            catch (Exception ex)
            {
                return NotFound($"Couldn't expand the schedule for the accommodation with id {id}. {ex.Message}");
            }
        }

        /// <summary>API PUT request to update an existing accommodation</summary>
        /// <param name="id">The ID of the accommodation to update.</param>
        /// <param name="accommodation">The updated accommodation object.</param>
        /// <returns>Status 200 and the updated accommodation if successful, otherwise status code 400</returns>
        [HttpPut]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> UpdateAccommodationAsync([FromRoute] int id, Accommodation accommodation)
        {
            try
            {
                await _accommodationService.UpdateAccommodationAsync(id, accommodation);
                return Ok(accommodation);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>API GET request to fetch all accommodations from the database.</summary>
        /// <returns>Status 200 and all the accommodations if successful, otherwise status code 404</returns>
        [HttpGet]
        [Route("api/[controller]")]
        public async Task<IActionResult> GetAllAccommodationsInTheSystemAsync()
        {
            try
            {
                return Ok(await _accommodationService.GetAllAccommodationsAsync());
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API GET request to fetch all the accommodations of a user</summary>
        /// <param name="userid">The ID of the user to return all the accommodations of.</param>
        /// <returns>Status 200 and all the accommodations owned by the user if successful, otherwise status code 404</returns>
        [HttpGet]
        [Route("api/[controller]/{userid}/allaccommodations")]
        public async Task<IActionResult> GetAllAccommodationsOfAUserAsync([FromRoute] int userid)
        {
            try
            {
                return Ok(await _accommodationService.GetAllOwnedAccommodationsAsync(userid));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API GET request to fetch an accommodation from the database</summary>
        /// <param name="id">The ID of the accommodation to get.</param>
        /// <returns>Status 200 and the requested accommodation if successful, otherwise status code 404</returns>
        [HttpGet]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> GetAccommodationAsync([FromRoute] int id)
        {
            try
            {
                return Ok(await _accommodationService.GetAccommodationAsync(id));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API DELETE request to delete an accommodation</summary>
        /// <param name="id">The ID of the accommodation to get.</param>
        /// <returns>Status 200 if successful, otherwise status code 400</returns>
        [HttpDelete]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> DeleteAccommodationAsync([FromRoute] int id)
        {
            try
            {
                await _deletionService.DeleteAccommodationAsync(id);
                return Ok("Deletion ok");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
