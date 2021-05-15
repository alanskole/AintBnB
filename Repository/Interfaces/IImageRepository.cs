using AintBnB.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AintBnB.Repository.Interfaces
{
    public interface IImageRepository
    {
        Task CreateAsync(Image img);
        Task DeleteAsync(int id);
        List<Image> GetAll(int accommodationId);
    }
}