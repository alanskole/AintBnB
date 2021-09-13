using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static AintBnB.BlazorWASM.Server.Helpers.CurrentUserDetails;
using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace AintBnB.BlazorWASM.Server.Controllers
{
    [ApiController]
    [Authorize]
    public class ImageController : Controller
    {
        private IImageService _imageService;

        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        /// <summary>API POST request to create a new image.</summary>
        /// <param name="img">The image object to create.</param>
        /// <returns>Status code 201 if successful, otherwise status 400</returns>
        [HttpPost]
        [Route("api/[controller]")]
        public async Task<IActionResult> CreateImageAsync([FromBody] Image img)
        {
            if (!CorrectUserOrAdmin(img.Accommodation.Owner.Id, GetIdOfLoggedInUser(HttpContext), GetUsertypeOfLoggedInUser(HttpContext)))
                return BadRequest("Only the accommodation's owner or admin can upload photos to an accommodation!");

            try
            {
                var newImg = await _imageService.AddPictureAsync(img.Accommodation.Id, img.Img);
                return CreatedAtAction(nameof(GetImageAsync), new { id = newImg.Id }, newImg);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>API GET request that gets an image.</summary>
        /// <param name="id">The Id of the image to fetch</param>
        /// <returns>Status 200 and the image if successful, otherwise status code 404</returns>
        [ActionName("GetImageAsync")]
        [HttpGet]
        [Route("api/[controller]/{id}")]
        public async Task<ActionResult<Image>> GetImageAsync([FromRoute] int id)
        {
            try
            {
                return await _imageService.GetPicture(id);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API GET request that gets all images of an accommodation.</summary>
        /// <param name="accommodationId">The Id of the accommodation to fetch the pictures of</param>
        /// <returns>Status 200 and all the images if successful, otherwise status code 404</returns>
        [HttpGet]
        [Route("api/[controller]/all/{accommodationId}")]
        public IActionResult GetAllImages([FromRoute] int accommodationId)
        {
            try
            {
                return Ok(_imageService.GetAllPictures(accommodationId));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API DELETE request to delete an image from the database.</summary>
        /// <param name="imageId">The ID of the image to delete.</param>
        /// <returns>Status 204 if successful, otherwise status code 404 or 400</returns>
        [HttpDelete]
        [Route("api/[controller]/{imageId}")]
        public async Task<IActionResult> DeleteImageAsync([FromRoute] int imageId)
        {
            try
            {
                var img = await _imageService.GetPicture(imageId);

                if (!CorrectUserOrAdmin(img.Accommodation.Owner.Id, GetIdOfLoggedInUser(HttpContext), GetUsertypeOfLoggedInUser(HttpContext)))
                    return BadRequest("Only the accommodation's owner or admin can remove photos from an accommodation!");

                await _imageService.RemovePictureAsync(imageId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
