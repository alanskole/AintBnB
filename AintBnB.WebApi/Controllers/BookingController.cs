using AintBnB.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static AintBnB.WebApi.Helpers.CurrentUserDetails;
using static AintBnB.BusinessLogic.Helpers.Authentication;
using AintBnB.BusinessLogic.CustomExceptions;

namespace AintBnB.WebApi.Controllers
{
    [ApiController]
    [Authorize]
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

        /// <summary>API GET request to make a booking.</summary>
        /// <param name="startDate">The start date of the booking.</param>
        /// <param name="bookerId">The user-ID of the booker.</param>
        /// <param name="nights">The amount of nights to book.</param>
        /// <param name="accommodationId">The ID of the accommodation to book.</param>
        /// <returns>Status 200 and the booking if successful, otherwise status code 400</returns>
        [HttpGet]
        [Route("api/[controller]/{startDate}/{bookerId}/{nights}/{accommodationId}")]
        public async Task<IActionResult> BookAsync([FromRoute] string startDate, [FromRoute] int bookerId, [FromRoute] int nights, [FromRoute] int accommodationId)
        {
            try
            {
                var accommodation = await _accommodationService.GetAccommodationAsync(accommodationId);
                var booker = await _userService.GetUserAsync(bookerId);

                if (!CheckIfUserIsAllowedToPerformAction(booker, accommodation.Owner.Id))
                    return BadRequest($"Must be performed by a customer with ID {booker.Id}, or by admin on behalf of a customer with ID {booker.Id}!");

                var booking = await _bookingService.BookAsync(startDate, booker, nights, accommodation);
                return Ok(booking);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>API GET request to update an existing booking.</summary>
        /// <param name="newStartDate">The updated start date of the booking.</param>
        /// <param name="nights">The amount of nights to book.</param>
        /// <param name="bookingId">The ID of the booking to update.</param>
        /// <returns>Status 200 and the updated booking if successful, otherwise status code 400</returns>
        [HttpGet]
        [Route("api/[controller]/{newStartDate}/{nights}/{bookingId}")]
        public async Task<IActionResult> UpdateBookingAsync([FromRoute] string newStartDate, [FromRoute] int nights, [FromRoute] int bookingId)
        {
            var currentUser = await _userService.GetUserAsync(GetIdOfLoggedInUser(HttpContext));
            var booking = await _bookingService.GetBookingAsync(bookingId);

            if (!CheckIfUserIsAllowedToPerformAction(currentUser, booking.Accommodation.Owner.Id))
                return BadRequest($"Must be performed by the booker, or by admin on behalf of the booker!");

            try
            {
                await _bookingService.UpdateBookingAsync(newStartDate, nights, bookingId);
                return Ok(booking);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>API GET request to leave a rating on a booking</summary>
        /// <param name="bookingId">The ID of the booking to leave a rating for</param>
        /// <param name="rating">The rating from 1-5</param>
        /// <returns>Status 200 if successful, otherwise status code 400</returns>
        [HttpGet]
        [Route("api/[controller]/rate/{bookingId}/{rating}")]
        public async Task<IActionResult> LeaveRatingAsync([FromRoute] int bookingId, [FromRoute] int rating)
        {
            var booker = await _bookingService.GetBookingAsync(bookingId);

            if (booker.Id != GetIdOfLoggedInUser(HttpContext))
                return BadRequest(new AccessException("Only the booker can leave a rating!").Message);

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

        /// <summary>API GET request to fetch a booking from the database</summary>
        /// <param name="id">The ID of the booking to get.</param>
        /// <returns>Status 200 and the requested booking if successful, otherwise status code 404</returns>
        [HttpGet]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> GetBookingAsync([FromRoute] int id)
        {
            try
            {
                var booking = await _bookingService.GetBookingAsync(id);

                if (CorrectUserOrOwnerOrAdmin(booking.Accommodation.Owner.Id, booking.BookedBy.Id, GetIdOfLoggedInUser(HttpContext), GetUsertypeOfLoggedInUser(HttpContext)))
                    return Ok(booking);
                else
                    return NotFound(new AccessException().Message);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API GET request to return all the bookings that are made on the accommodations of a user</summary>
        /// <param name="id">The ID of the user to return the bookings of their accommodations of.</param>
        /// <returns>Status 200 and the all the bookings if successful, otherwise status code 404</returns>
        [HttpGet]
        [Route("api/[controller]/{id}/bookingsownaccommodation")]
        public async Task<IActionResult> GetBookingsOnOwnedAccommodationsAsync([FromRoute] int id)
        {
            var user = await _userService.GetUserAsync(id);

            if (CorrectUserOrAdmin(user.Id, GetIdOfLoggedInUser(HttpContext), GetUsertypeOfLoggedInUser(HttpContext)))
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
            else
                return NotFound(new AccessException().Message);
        }

        /// <summary>API GET request to return all the bookings from the database</summary>
        /// <returns>Status 200 and all the bookings if successful, otherwise status code 404</returns>
        [HttpGet]
        [Route("api/[controller]")]
        public async Task<IActionResult> GetAllBookingsAsync()
        {
            try
            {
                var user = await _userService.GetUserAsync(GetIdOfLoggedInUser(HttpContext));

                if (AdminChecker(user.UserType))
                    return Ok(await _bookingService.GetAllInSystemAsync());
                else
                    return Ok(await _bookingService.GetOnlyOnesOwnedByUserAsync(GetIdOfLoggedInUser(HttpContext)));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API DELETE request to delete a booking</summary>
        /// <param name="id">The ID of the booking to cancel.</param>
        /// <returns>Status 200 if successful, otherwise status code 400</returns>
        [HttpDelete]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> DeleteBookingAsync([FromRoute] int id)
        {
            try
            {
                var booking = await _bookingService.GetBookingAsync(id);

                if (!CorrectUserOrOwnerOrAdmin(booking.Accommodation.Owner.Id, booking.BookedBy.Id, GetIdOfLoggedInUser(HttpContext), GetUsertypeOfLoggedInUser(HttpContext)))
                    return BadRequest(new AccessException().Message);

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
