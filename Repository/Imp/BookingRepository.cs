using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

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
            return _databaseContext.Booking.Include(bk => bk.Accommodation).ThenInclude(ac => ac.Address).ToList();
        }

        public Booking Read(int id)
        {
            return _databaseContext.Booking.Include(bk => bk.Accommodation).ThenInclude(ac => ac.Address).FirstOrDefault(bk => bk.Id == id);
        }

        public void Update(int id, Booking updatedBooking)
        {
            Booking oldBooking = Read(id);
            oldBooking.Dates = updatedBooking.Dates;
            oldBooking.Price = updatedBooking.Price;
            oldBooking.Rating = updatedBooking.Rating;
        }
    }
}
