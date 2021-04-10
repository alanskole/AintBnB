using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AintBnB.Repository.Imp
{
    public class BookingRepository : IRepository<Booking>
    {
        private DatabaseContext _databaseContext;

        public BookingRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        public async Task CreateAsync(Booking booking)
        {
            await _databaseContext.Booking.AddAsync(booking);
        }

        public async Task DeleteAsync(int id)
        {
            _databaseContext.Booking.Remove(await ReadAsync(id));
        }

        public async Task<List<Booking>> GetAllAsync()
        {
            return await _databaseContext.Booking.Include(bk => bk.BookedBy).Include(bk => bk.Accommodation).ThenInclude(ac => ac.Address).Include(bk => bk.Accommodation.Owner).ToListAsync();
        }

        public async Task<Booking> ReadAsync(int id)
        {
            return await _databaseContext.Booking.Include(bk => bk.BookedBy).Include(bk => bk.Accommodation).ThenInclude(ac => ac.Address).Include(bk => bk.Accommodation.Owner).FirstOrDefaultAsync(bk => bk.Id == id);
        }

        public async Task UpdateAsync(int id, Booking updatedBooking)
        {
            var oldBooking = await ReadAsync(id);
            oldBooking.Dates = updatedBooking.Dates;
            oldBooking.Price = updatedBooking.Price;
            oldBooking.Rating = updatedBooking.Rating;
        }
    }
}
