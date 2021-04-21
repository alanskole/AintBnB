using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AintBnB.Repository.Imp
{
    public class AccommodationRepository : IRepository<Accommodation>
    {
        private DatabaseContext _databaseContext;

        public AccommodationRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task CreateAsync(Accommodation accommodation)
        {
            await _databaseContext.Address.AddAsync(accommodation.Address);
            await _databaseContext.Accommodation.AddAsync(accommodation);
        }

        public async Task DeleteAsync(int id)
        {
            var acc = await ReadAsync(id);
            _databaseContext.Address.Remove(acc.Address);
            _databaseContext.Accommodation.Remove(acc);
        }

        public async Task<List<Accommodation>> GetAllAsync()
        {
            return await _databaseContext.Accommodation.Include(ac => ac.Address).Include(ac => ac.Owner).ToListAsync();
        }

        public async Task<Accommodation> ReadAsync(int id)
        {
            return await _databaseContext.Accommodation.Include(ac => ac.Address).Include(ac => ac.Owner).FirstOrDefaultAsync(ac => ac.Id == id);
        }

        public async Task UpdateAsync(int id, Accommodation accommodation)
        {
            var acc = await ReadAsync(id);
            acc.SquareMeters = accommodation.SquareMeters;
            acc.AmountOfBedrooms = accommodation.AmountOfBedrooms;
            acc.Description = accommodation.Description;
            acc.PricePerNight = accommodation.PricePerNight;
            acc.Picture = accommodation.Picture;
            acc.CancellationDeadlineInDays = accommodation.CancellationDeadlineInDays;
            if (accommodation.Schedule != null)
                acc.Schedule = new SortedDictionary<string, bool>(accommodation.Schedule);
        }
    }
}
