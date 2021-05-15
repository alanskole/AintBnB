using AintBnB.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AintBnB.BusinessLogic.Interfaces
{
    public interface IImageService
    {
        Task<Image> AddPictureAsync(int accommoationId, byte[] img);
        Task RemovePictureAsync(int id);
        List<Image> GetAllPictures(int accommoationId);
    }
}
