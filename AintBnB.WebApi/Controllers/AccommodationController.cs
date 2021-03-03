using AintBnB.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using AintBnB.BusinessLogic.Interfaces;
using System.Collections.Generic;

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
        public IActionResult CreateAccommodation([FromRoute] int days, [FromRoute] int userId, [FromBody] Accommodation accommodation)
        {
            try
            {
                User owner = _userService.GetUser(userId);
                Accommodation newAccommodation = _accommodationService.CreateAccommodation(owner, accommodation.Address, accommodation.SquareMeters, accommodation.AmountOfBedrooms, accommodation.KilometersFromCenter, accommodation.Description, accommodation.PricePerNight, accommodation.CancellationDeadlineInDays, accommodation.Picture, days);
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
        public IActionResult ExpandSchedule([FromRoute] int id, [FromRoute] int days)
        {
            try
            {
                _accommodationService.ExpandScheduleOfAccommodationWithXAmountOfDays(id, days);
                return Ok(_accommodationService.GetAccommodation(id));
            }
            catch (Exception ex)
            {
                return NotFound($"Couldn't expand the schedule for the accommodation with id {id}. {ex.Message}");
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
        public IActionResult GetAllAccommodationsInTheSystem()
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
        [Route("api/[controller]/{userid}/allaccommodations")]
        public IActionResult GetAllAccommodationsOfAUser([FromRoute] int userid)
        {
            try
            {
                return Ok(_accommodationService.GetAllOwnedAccommodations(userid));
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
