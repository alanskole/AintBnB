using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AintBnB.WebApi.Controllers
{
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
            try
            {
                var newImg = await _imageService.AddPictureAsync(img.Accommodation.Id, img.Img);
                return Created(HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + HttpContext.Request.Path + "/" + newImg.Id, img);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>API GET request that gets all images of an accommodation.</summary>
        /// <returns>Status 200 and all the images if successful, otherwise status code 404</returns>
        [HttpGet]
        [Route("api/[controller]/{accommodationId}")]
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
        /// <returns>Status 200 if successful, otherwise status code 404</returns>
        [HttpDelete]
        [Route("api/[controller]/{imageId}")]
        public async Task<IActionResult> DeleteImageAsync([FromRoute] int imageId)
        {
            try
            {
                await _imageService.RemovePictureAsync(imageId);
                return Ok("Deleted");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
