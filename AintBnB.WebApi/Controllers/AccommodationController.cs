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
            catch (Exception)
            {
                return NotFound("Accommodation could not be created");
            }
        }

        [HttpGet]
        [Route("api/[controller]/{country}/{municipality}/{startdate}/{nights}")]
        public IActionResult FindAvailable([FromRoute] string municipality, [FromRoute] string country, [FromRoute] string startdate, [FromRoute] int nights)
        {
            try
            {
                List<Accommodation> result = _accommodationService.FindAvailable(country, municipality, startdate, nights);
                return Ok(result);
            }
            catch (Exception)
            {
                return NotFound($"No available accommodations found in {country}, {municipality} from {startdate} for {nights} nights");
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
            catch (Exception)
            {
                return NotFound($"Couldn't expand the schedule for the accommodation with id {id}");
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
            catch (Exception)
            {
                return NotFound($"Accommodation with id {id} could not be updated");
            }
        }

        [HttpGet]
        [Route("api/[controller]")]
        public IActionResult GetAllAccommodations()
        {
            return Ok(_accommodationService.GetAllAccommodations());
        }

        [HttpGet]
        [Route("api/[controller]/{id}")]
        public IActionResult GetAccommodation([FromRoute] int id)
        {
            try
            {
                return Ok(_accommodationService.GetAccommodation(id));
            }
            catch (Exception)
            {
                return NotFound($"Accommodation with id {id} could not be found");
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
            catch (Exception)
            {
                return NotFound($"Accommodation with id {id} could not be deleted");
            }
        }
    }
}
