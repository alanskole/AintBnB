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
        public async Task<Image> AddPictureAsync(int accommoationId, byte[] img)
        {
            var acc = await _unitOfWork.AccommodationRepository.ReadAsync(accommoationId);
            var newImage = new Image(acc, img);
            await _unitOfWork.ImageRepository.CreateAsync(newImage);
            await _unitOfWork.CommitAsync();
            return newImage;
        }

        /// <summary>Gets a list of all the images of an accommodation.</summary>
        /// <param name="accommoationId">The Id of the accommodation to get all the images of.</param>
        public List<Image> GetAllPictures(int accommoationId)
        {
            return _unitOfWork.ImageRepository.GetAll(accommoationId);
        }

        /// <summary>Deletes an image from the list of images of an accommodation.</summary>
        /// <param name="imageId">The Id of the image to delete.</param>
        public async Task RemovePictureAsync(int imageId)
        {
            await _unitOfWork.ImageRepository.DeleteAsync(imageId);
            await _unitOfWork.CommitAsync();
        }
    }
}
