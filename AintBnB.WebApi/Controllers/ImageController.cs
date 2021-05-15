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
