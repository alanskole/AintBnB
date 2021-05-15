using AintBnB.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AintBnB.Repository.Interfaces
{
    public interface IImageRepository
    {
        Task CreateAsync(Image img);
        void Delete(Image img);
        List<Image> GetAll(int accommodationId);
        Task<Image> ReadAsync(int imageId);
    }
}