using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.BusinessLogic.DependencyProviderFactory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AintBnB.BusinessLogic.Repository
{
    public class BookingRepository : IRepository<Booking>
    {
        public readonly DatabaseContext _databaseContext = ProvideDependencyFactory.databaseContext;

        public void Create(Booking booking)
        {
            _databaseContext.Booking.Add(booking);
            _databaseContext.SaveChanges();
        }

        public void Delete(int id)
        {
            _databaseContext.Booking.Remove(Read(id));
            _databaseContext.SaveChanges();
        }

        public List<Booking> GetAll()
        {
            return _databaseContext.Booking.Include(bk => bk.Accommodation).ThenInclude(ac => ac.Address).ToList();
        }

        public Booking Read(int id)
        {
            return _databaseContext.Booking.Include(bk => bk.Accommodation).ThenInclude(ac => ac.Address).FirstOrDefault(bk => bk.Id == id);
        }

        public void Update(int id, Booking t)
        {
            throw new NotImplementedException();
        }
    }
}
