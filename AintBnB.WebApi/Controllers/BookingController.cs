using AintBnB.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AintBnB.WebApi.Controllers
{
    [ApiController]
    public class BookingController : ControllerBase
    {
        private IBookingService _bookingService;
        private IUserService _userService;
        private IAccommodationService _accommodationService;
        private IDeletionService _deletionService;

        public BookingController(IBookingService bookingService, IUserService userService, IAccommodationService accommodationService, IDeletionService deletionService)
        {
            _bookingService = bookingService;
            _userService = userService;
            _accommodationService = accommodationService;
            _deletionService = deletionService;
        }

        [HttpGet]
        [Route("api/[controller]/{startDate}/{bookerId}/{nights}/{accommodationId}")]
        public async Task<IActionResult> BookAsync([FromRoute] string startDate, [FromRoute] int bookerId, [FromRoute] int nights, [FromRoute] int accommodationId)
        {
            try
            {
                var booker = await _userService.GetUserAsync(bookerId);
                var accommodation = await _accommodationService.GetAccommodationAsync(accommodationId);
                var booking = await _bookingService.BookAsync(startDate, booker, nights, accommodation);
                return Ok(booking);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/[controller]/{newStartDate}/{nights}/{bookingId}")]
        public async Task<IActionResult> UpdateBookingAsync([FromRoute] string newStartDate, [FromRoute] int nights, [FromRoute] int bookingId)
        {
            try
            {
                var booking = await _bookingService.GetBookingAsync(bookingId);
                await _bookingService.UpdateBookingAsync(newStartDate, nights, bookingId);
                return Ok(booking);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/[controller]/rate/{bookingId}/{rating}")]
        public async Task<IActionResult> LeaveRatingAsync([FromRoute] int bookingId, [FromRoute] int rating)
        {
            try
            {
                await _bookingService.RateAsync(bookingId, rating);
                return Ok("Rated");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> GetBookingAsync([FromRoute] int id)
        {
            try
            {
                return Ok(await _bookingService.GetBookingAsync(id));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/[controller]/{id}/bookingsownaccommodation")]
        public async Task<IActionResult> GetBookingsOnOwnedAccommodationsAsync([FromRoute] int id)
        {
            try
            {
                return Ok(await _bookingService.GetBookingsOfOwnedAccommodationAsync(id));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/[controller]")]
        public async Task<IActionResult> GetAllBookingsAsync()
        {
            try
            {
                return Ok(await _bookingService.GetAllBookingsAsync());
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> DeleteBookingAsync([FromRoute] int id)
        {
            try
            {
                await _deletionService.DeleteBookingAsync(id);
                return Ok("Deletion ok");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
