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

        /// <summary>API POST request to make a booking.</summary>
        /// <param name="startDate">The start date of the booking.</param>
        /// <param name="bookerId">The user-ID of the booker.</param>
        /// <param name="nights">The amount of nights to book.</param>
        /// <param name="accommodationId">The ID of the accommodation to book.</param>
        /// <returns>Status 200 and the booking if successful, otherwise status code 400</returns>
        [HttpPost]
        [Route("api/[controller]/book")]
        public async Task<IActionResult> BookAsync([FromBody] string[] bookingInfo)
        {
            try
            {
                var accommodation = await _accommodationService.GetAccommodationAsync(int.Parse(bookingInfo[3]));
                var booker = await _userService.GetUserAsync(int.Parse(bookingInfo[1]));

                if (!CheckIfUserIsAllowedToPerformAction(booker, GetIdOfLoggedInUser(HttpContext), GetUsertypeOfLoggedInUser(HttpContext)))
                    return BadRequest($"Must be performed by a customer with ID {booker.Id}, or by admin on behalf of a customer with ID {booker.Id}!");

                var booking = await _bookingService.BookAsync(bookingInfo[0], booker, int.Parse(bookingInfo[2]), accommodation);
                return CreatedAtAction(nameof(GetBookingAsync), new { id = booking.Id }, booking);
            }
            catch (Exception ex)
            {
                if (ex.GetType().IsAssignableFrom(typeof(NotFoundException)))
                    return NotFound(ex.Message);
                else
                    return BadRequest(ex.Message);
            }
        }

        /// <summary>API PUT request to update an existing booking.</summary>
        /// <param name="newDates">The updated start date of the booking and the amount of nights to book for in a list.</param>
        /// <param name="bookingId">The ID of the booking to update.</param>
        /// <returns>Status 204 if successful, otherwise status code 400 or 404</returns>
        [HttpPut]
        [Route("api/[controller]/{bookingId}")]
        public async Task<IActionResult> UpdateBookingAsync([FromBody] string[] newDates, [FromRoute] int bookingId)
        {
            try
            {
                var booking = await _bookingService.GetBookingAsync(bookingId);

                if (!CheckIfUserIsAllowedToPerformAction(booking.BookedBy, GetIdOfLoggedInUser(HttpContext), GetUsertypeOfLoggedInUser(HttpContext)))
                    return BadRequest($"Must be performed by the booker, or by admin on behalf of the booker!");

                await _bookingService.UpdateBookingAsync(newDates[0], int.Parse(newDates[1]), bookingId);
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

        /// <summary>API PUT request to leave a rating on a booking</summary>
        /// <param name="bookingId">The ID of the booking to leave a rating for</param>
        /// <param name="rating">The rating from 1-5</param>
        /// <returns>Status 204 if successful, otherwise status code 400 or 404</returns>
        [HttpPut]
        [Route("api/[controller]/rate/{bookingId}")]
        public async Task<IActionResult> LeaveRatingAsync([FromRoute] int bookingId, [FromBody] int rating)
        {
            try
            {
                var booking = await _bookingService.GetBookingAsync(bookingId);

                if (booking.BookedBy.Id != GetIdOfLoggedInUser(HttpContext))
                    return BadRequest("Only the booker can leave a rating!");

                await _bookingService.RateAsync(bookingId, rating);
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

        /// <summary>API GET request to fetch a booking from the database</summary>
        /// <param name="id">The ID of the booking to get.</param>
        /// <returns>Status 200 and the requested booking if successful, otherwise status code 400 or 404</returns>
        [ActionName("GetBookingAsync")]
        [HttpGet]
        [Route("api/[controller]/{id}")]
        public async Task<ActionResult<Booking>> GetBookingAsync([FromRoute] int id)
        {
            try
            {
                var booking = await _bookingService.GetBookingAsync(id);

                if (!CorrectUserOrOwnerOrAdmin(booking.Accommodation.Owner.Id, booking.BookedBy.Id, GetIdOfLoggedInUser(HttpContext), GetUsertypeOfLoggedInUser(HttpContext)))
                    return BadRequest("Restricted access!");

                return booking;
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API GET request to return all the bookings that are made on the accommodations of a user</summary>
        /// <param name="id">The ID of the user to return the bookings of their accommodations of.</param>
        /// <returns>Status 200 and the all the bookings if successful, otherwise status code 404 or 400</returns>
        [HttpGet]
        [Route("api/[controller]/{id}/bookingsownaccommodation")]
        public async Task<ActionResult<List<Booking>>> GetBookingsOnOwnedAccommodationsAsync([FromRoute] int id)
        {
            try
            {
                var user = await _userService.GetUserAsync(id);

                if (!CorrectUserOrAdmin(user.Id, GetIdOfLoggedInUser(HttpContext), GetUsertypeOfLoggedInUser(HttpContext)))
                    return BadRequest("Restricted access!");

                return await _bookingService.GetBookingsOfOwnedAccommodationAsync(id);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API GET request to return all the bookings from the database</summary>
        /// <returns>Status 200 and all the bookings if successful, otherwise status code 404</returns>
        [HttpGet]
        [Route("api/[controller]")]
        public async Task<ActionResult<List<Booking>>> GetAllBookingsAsync()
        {
            try
            {
                var user = await _userService.GetUserAsync(GetIdOfLoggedInUser(HttpContext));

                if (AdminChecker(user.UserType))
                    return await _bookingService.GetAllInSystemAsync();
                else
                    return await _bookingService.GetOnlyOnesOwnedByUserAsync(GetIdOfLoggedInUser(HttpContext));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API DELETE request to delete a booking</summary>
        /// <param name="id">The ID of the booking to cancel.</param>
        /// <returns>Status 204 if successful, otherwise status code 400 or 404</returns>
        [HttpDelete]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> DeleteBookingAsync([FromRoute] int id)
        {
            try
            {
                var booking = await _bookingService.GetBookingAsync(id);

                if (!CorrectUserOrOwnerOrAdmin(booking.Accommodation.Owner.Id, booking.BookedBy.Id, GetIdOfLoggedInUser(HttpContext), GetUsertypeOfLoggedInUser(HttpContext)))
                    return BadRequest("Restricted access, action can only be done by the booker, owner of the accommodation or admin!");

                await _deletionService.DeleteBookingAsync(id);

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
