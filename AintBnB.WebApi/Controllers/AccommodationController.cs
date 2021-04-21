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
