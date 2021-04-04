using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace AintBnB.Repository.Imp
{
    public class BookingRepository : IRepository<Booking>
    {
        private DatabaseContext _databaseContext;

        public BookingRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        public void Create(Booking booking)
        {
            _databaseContext.Booking.Add(booking);
        }

        public void Delete(int id)
        {
            _databaseContext.Booking.Remove(Read(id));
        }

        public List<Booking> GetAll()
        {
            return _databaseContext.Booking.Include(bk => bk.BookedBy).Include(bk => bk.Accommodation).ThenInclude(ac => ac.Address).Include(bk => bk.Accommodation.Owner).ToList();
        }

        public Booking Read(int id)
        {
            return _databaseContext.Booking.Include(bk => bk.BookedBy).Include(bk => bk.Accommodation).ThenInclude(ac => ac.Address).Include(bk => bk.Accommodation.Owner).FirstOrDefault(bk => bk.Id == id);
        }

        public void Update(int id, Booking updatedBooking)
        {
            var oldBooking = Read(id);
            oldBooking.Dates = updatedBooking.Dates;
            oldBooking.Price = updatedBooking.Price;
            oldBooking.Rating = updatedBooking.Rating;
        }
    }
}
