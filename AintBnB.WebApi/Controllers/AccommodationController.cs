using AintBnB.Core.Models;
using AintBnB.BusinessLogic.DependencyProviderFactory;
using AintBnB.BusinessLogic.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace AintBnB.WebApi.Controllers
{
    [ApiController]
    public class AccommodationController : ControllerBase
    {
        private IAccommodationService _accommodationService;
        private IDeletionService _deletionService;

        public AccommodationController()
        {
            _accommodationService = ProvideDependencyFactory.accommodationService;
            _deletionService = ProvideDependencyFactory.deletionService;
        }

        [HttpPost]
        [Route("api/[controller]/{days}/{userId}")]
        public IActionResult CreateAccommodation([FromRoute] int days, [FromRoute] int userId, Accommodation accommodation)
        {
            try
            {
                User owner = ProvideDependencyFactory.userService.GetUser(userId);
                Accommodation newAccommodation = _accommodationService.CreateAccommodation(owner, accommodation.Address, accommodation.SquareMeters, accommodation.AmountOfBedrooms, accommodation.KilometersFromCenter, accommodation.Description, accommodation.PricePerNight, days);
                return Created(HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + HttpContext.Request.Path + "/" + accommodation.Id, accommodation);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/[controller]/{country}/{city}/{startdate}/{nights}")]
        public IActionResult FindAvailable([FromRoute] string city, [FromRoute] string country, [FromRoute] string startdate, [FromRoute] int nights)
        {
            try
            {
                return Ok(_accommodationService.FindAvailable(country, city, startdate, nights));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/[controller]/{id}/{days}")]
        public IActionResult ExpandSchedule([FromRoute] int id, [FromRoute] int days)
        {
            try
            {
                _accommodationService.ExpandScheduleOfAccommodationWithXAmountOfDays(id, days);
                return Ok(_accommodationService.GetAccommodation(id));
            }
            catch (Exception ex)
            {
                return BadRequest($"Couldn't expand the schedule for the accommodation with id {id}. {ex.Message}");
            }
        }

        [HttpPut]
        [Route("api/[controller]/{id}")]
        public IActionResult UpdateAccommodation([FromRoute] int id, Accommodation accommodation)
        {
            try
            {
                _accommodationService.UpdateAccommodation(id, accommodation);
                return Ok(accommodation);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/[controller]")]
        public IActionResult GetAllAccommodations()
        {
            try
            {
                return Ok(_accommodationService.GetAllAccommodations());
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/[controller]/{id}")]
        public IActionResult GetAccommodation([FromRoute] int id)
        {
            try
            {
                return Ok(_accommodationService.GetAccommodation(id));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete]
        [Route("api/[controller]/{id}")]
        public IActionResult DeleteAccommodation([FromRoute] int id)
        {
            try
            {
                _deletionService.DeleteAccommodation(id);
                return Ok("Deletion ok");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
