using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Interfaces;
using AintBnB.Core.Models;
using AintBnB.Repository.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AintBnB.BusinessLogic.Imp
{
    public class ImageService : IImageService
    {
        private IUnitOfWork _unitOfWork;

        public ImageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        /// <summary>Adds a new image to the accommodation.</summary>
        /// <param name="accommoationId">The Id of the accommodation the image will be added to.</param>
        /// <param name="img">The byte array representing the image to add.</param>
        /// <returns>The newly created image object</returns>
        public async Task<Image> AddPictureAsync(int accommoationId, byte[] img)
        {
            var acc = await _unitOfWork.AccommodationRepository.ReadAsync(accommoationId);

            var newImage = new Image(acc, img);
            await _unitOfWork.ImageRepository.CreateAsync(newImage);
            await _unitOfWork.CommitAsync();
            return newImage;
        }

        /// <summary>Gets an image by id.</summary>
        /// <param name="imageId">The Id of the image to fetch.</param>
        /// <returns>An image object with the requested image</returns>
        public async Task<Image> GetPicture(int imageId)
        {
            var img = await _unitOfWork.ImageRepository.ReadAsync(imageId);

            if (img == null)
                throw new NotFoundException("Image", imageId);

            return img;
        }

        /// <summary>Gets a list of all the images of an accommodation.</summary>
        /// <param name="accommoationId">The Id of the accommodation to get all the images of.</param>
        /// <returns>A list with all the images of an accommodation</returns>
        public List<Image> GetAllPictures(int accommoationId)
        {
            var all = _unitOfWork.ImageRepository.GetAll(accommoationId);

            if (all == null)
                throw new NotFoundException("images");

            return all;
        }
    }
}
