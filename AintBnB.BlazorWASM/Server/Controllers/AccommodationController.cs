using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AintBnB.BlazorWASM.Server.Helpers.CurrentUserDetails;
using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace AintBnB.BlazorWASM.Server.Controllers
{
    [ApiController]
    [Authorize]
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

                if (!CheckIfUserIsAllowedToPerformAction(owner, GetIdOfLoggedInUser(HttpContext), GetUsertypeOfLoggedInUser(HttpContext)))
                    return BadRequest("Restricted acces, this action can only be done by administrator or the user that will be the owner of the accommodation!");

                var newAccommodation = await _accommodationService.CreateAccommodationAsync(owner, accommodation.Address, accommodation.SquareMeters, accommodation.AmountOfBedrooms, accommodation.KilometersFromCenter, accommodation.Description, accommodation.PricePerNight, accommodation.CancellationDeadlineInDays, days);
                return CreatedAtAction(nameof(GetAccommodationAsync), new { id = newAccommodation.Id }, newAccommodation);
            }
            catch (Exception ex)
            {
                if (ex.GetType().IsAssignableFrom(typeof(NotFoundException)))
                    return NotFound(ex.Message);
                else
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
        public async Task<ActionResult<List<Accommodation>>> FindAvailableAsync([FromRoute] string city, [FromRoute] string country, [FromRoute] string startdate, [FromRoute] int nights)
        {
            try
            {
                return await _accommodationService.FindAvailableAsync(country, city, startdate, nights);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API PUT request to sort a list of accommodations in a specific order</summary>
        /// <param name="available">The list of accommodations that must be sorted.</param>
        /// <param name="sortBy">The order to sort by: Price, Distance, Rating or Size.</param>
        /// <param name="ascOrDesc">Sort in ascending or descending order</param>
        /// <returns>Status 200 and the correct sorting order if successful, otherwise status code 400</returns>
        [HttpPut]
        [Route("api/[controller]/sort/{sortBy}/{ascOrDesc}")]
        [IgnoreAntiforgeryToken]
        public ActionResult<List<Accommodation>> SortAvailableList([FromBody] List<Accommodation> available, [FromRoute] string sortBy, [FromRoute] string ascOrDesc)
        {
            try
            {
                return _accommodationService.SortListOfAccommodations(available, sortBy, ascOrDesc);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>API PUT request to expand the schedule of an accommodation by x amount of days</summary>
        /// <param name="id">The ID of the accommodation to expand the schedule of.</param>
        /// <param name="days">The amount of days to expand the schedule by.</param>
        /// <returns>Status 204 and the accommodation if successful, otherwise status code 400 or 404</returns>
        [HttpPut]
        [Route("api/[controller]/{id}/expand")]
        public async Task<IActionResult> ExpandScheduleAsync([FromRoute] int id, [FromBody] int days)
        {
            try
            {
                var acc = await _accommodationService.GetAccommodationAsync(id);

                if (!CorrectUserOrAdmin(acc.Owner.Id, GetIdOfLoggedInUser(HttpContext), GetUsertypeOfLoggedInUser(HttpContext)))
                    return BadRequest("Restricted acces, this action can only be done by administrator or owner of the accommodation!");

                await _accommodationService.ExpandScheduleOfAccommodationWithXAmountOfDaysAsync(id, days);
                return NoContent();
            }
            catch (Exception ex)
            {
                if (ex.GetType().IsAssignableFrom(typeof(NotFoundException)))
                    return NotFound(ex.Message);
                else
                    return BadRequest(ex.Message);
            }
        }

        /// <summary>API PUT request to update an existing accommodation</summary>
        /// <param name="id">The ID of the accommodation to update.</param>
        /// <param name="accommodation">The updated accommodation object.</param>
        /// <returns>Status 204 and the updated accommodation if successful, otherwise status code 400</returns>
        [HttpPut]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> UpdateAccommodationAsync([FromRoute] int id, Accommodation accommodation)
        {
            try
            {
                var acc = await _accommodationService.GetAccommodationAsync(id);
                var userType = GetUsertypeOfLoggedInUser(HttpContext);

                if (!CorrectUserOrAdmin(acc.Owner.Id, GetIdOfLoggedInUser(HttpContext), userType))
                    return BadRequest("Restricted access!");

                await _accommodationService.UpdateAccommodationAsync(id, accommodation);
                return NoContent();
            }
            catch (Exception ex)
            {
                if (ex.GetType().IsAssignableFrom(typeof(NotFoundException)))
                    return NotFound(ex.Message);
                else
                    return BadRequest(ex.Message);
            }
        }

        /// <summary>API GET request to fetch all accommodations from the database.</summary>
        /// <returns>Status 200 and all the accommodations if successful, otherwise status code 404</returns>
        [HttpGet]
        [Route("api/[controller]")]
        public async Task<ActionResult<List<Accommodation>>> GetAllAccommodationsInTheSystemAsync()
        {
            try
            {
                return await _accommodationService.GetAllAccommodationsAsync();
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
        public async Task<ActionResult<List<Accommodation>>> GetAllAccommodationsOfAUserAsync([FromRoute] int userid)
        {
            try
            {
                return await _accommodationService.GetAllOwnedAccommodationsAsync(userid);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API GET request to fetch an accommodation from the database</summary>
        /// <param name="id">The ID of the accommodation to get.</param>
        /// <returns>Status 200 and the requested accommodation if successful, otherwise status code 404</returns>
        [ActionName("GetAccommodationAsync")]
        [HttpGet]
        [Route("api/[controller]/{id}")]
        public async Task<ActionResult<Accommodation>> GetAccommodationAsync([FromRoute] int id)
        {
            try
            {
                return await _accommodationService.GetAccommodationAsync(id);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API DELETE request to delete an accommodation</summary>
        /// <param name="id">The ID of the accommodation to get.</param>
        /// <returns>Status 204 if successful, otherwise status code 400</returns>
        [HttpDelete]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> DeleteAccommodationAsync([FromRoute] int id)
        {
            try
            {
                var accommodation = await _accommodationService.GetAccommodationAsync(id);

                if (!CorrectUserOrAdmin(accommodation.Owner.Id, GetIdOfLoggedInUser(HttpContext), GetUsertypeOfLoggedInUser(HttpContext)))
                    return BadRequest("Restricted acces, this action can only be done by administrator or owner of the accommodation!");

                await _deletionService.DeleteAccommodationAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                if (ex.GetType().IsAssignableFrom(typeof(NotFoundException)))
                    return NotFound(ex.Message);
                else
                    return BadRequest(ex.Message);
            }
        }
    }
}
