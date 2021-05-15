using AintBnB.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AintBnB.BusinessLogic.Interfaces
{
    public interface IImageService
    {
        /// <summary>Adds a new image to the accommodation.</summary>
        /// <param name="accommoationId">The Id of the accommodation the image will be added to.</param>
        /// <param name="img">The byte array representing the image to add.</param>
        /// <returns>The newly created image object</returns>
        Task<Image> AddPictureAsync(int accommoationId, byte[] img);

        /// <summary>Deletes an image from the list of images of an accommodation.</summary>
        /// <param name="imageId">The Id of the image to delete.</param>
        Task RemovePictureAsync(int imageId);

        /// <summary>Gets a list of all the images of an accommodation.</summary>
        /// <param name="accommoationId">The Id of the accommodation to get all the images of.</param>
        /// <returns>A list with all the images of an accommodation</returns>
        List<Image> GetAllPictures(int accommoationId);
    }
}
