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

        public async Task<Image> AddPictureAsync(int accommoationId, byte[] img)
        {
            var acc = await _unitOfWork.AccommodationRepository.ReadAsync(accommoationId);
            var newImage = new Image(acc, img);
            await _unitOfWork.ImageRepository.CreateAsync(newImage);
            await _unitOfWork.CommitAsync();
            return newImage;
        }

        public List<Image> GetAllPictures(int accommoationId)
        {
            return _unitOfWork.ImageRepository.GetAll(accommoationId);
        }

        public async Task RemovePictureAsync(int id)
        {
            await _unitOfWork.ImageRepository.DeleteAsync(id);
            await _unitOfWork.CommitAsync();
        }
    }
}
