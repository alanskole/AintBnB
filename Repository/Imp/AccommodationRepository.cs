using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using AintBnB.Repository.Interfaces;

namespace AintBnB.Repository.Imp
{
    public class AccommodationRepository : IRepository<Accommodation>
    {
        private DatabaseContext _databaseContext;

        public AccommodationRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public void Create(Accommodation accommodation)
        {
            _databaseContext.Address.Add(accommodation.Address);
            _databaseContext.Accommodation.Add(accommodation);
        }

        public void Delete(int id)
        {
            var acc = _databaseContext.Accommodation.Find(id);
            _databaseContext.Address.Remove(acc.Address);
            _databaseContext.Accommodation.Remove(Read(id));
        }

        public List<Accommodation> GetAll()
        {
            return _databaseContext.Accommodation.Include(ac => ac.Address).Include(ac => ac.Owner).ToList();
        }

        public Accommodation Read(int id)
        {
            return _databaseContext.Accommodation.Include(ac => ac.Address).Include(ac => ac.Owner).FirstOrDefault(ac => ac.Id == id);
        }

        public void Update(int id, Accommodation accommodation)
        {
            var acc = _databaseContext.Accommodation.Find(id);
            acc.SquareMeters = accommodation.SquareMeters;
            acc.AmountOfBedrooms = accommodation.AmountOfBedrooms;
            acc.Description = accommodation.Description;
            acc.PricePerNight = accommodation.PricePerNight;
            acc.Picture = accommodation.Picture;
            acc.CancellationDeadlineInDays = accommodation.CancellationDeadlineInDays;
            acc.Schedule = new SortedDictionary<string, bool>(accommodation.Schedule);
        }
    }
}
