using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.Repository.Interfaces;
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

        public async Task DeleteAsync(int id)
        {
            _databaseContext.Image.Remove(await _databaseContext.Image.FindAsync(id));
        }

        public List<Image> GetAll(int accommodationId)
        {
            return _databaseContext.Image.Where(i => i.Accommodation.Id == accommodationId).ToList();
        }
    }
}
