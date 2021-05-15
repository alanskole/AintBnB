using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AintBnB.Repository.Imp
{
    public class ImageRepository : IImageRepository
    {
        private DatabaseContext _databaseContext;

        public ImageRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task CreateAsync(Image img)
        {
            await _databaseContext.Image.AddAsync(img);
        }

        public void Delete(Image img)
        {
            _databaseContext.Image.Remove(img);
        }

        public List<Image> GetAll(int accommodationId)
        {
            return _databaseContext.Image.Where(i => i.Accommodation.Id == accommodationId).ToList();
        }

        public async Task<Image> ReadAsync(int imageId)
        {
            return await _databaseContext.Image.Include(i => i.Accommodation).ThenInclude(i => i.Owner).FirstOrDefaultAsync(i => i.Id == imageId);
        }
    }
}
