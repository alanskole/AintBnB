using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.BusinessLogic.DependencyProviderFactory;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AintBnB.BusinessLogic.Repository
{
    public class AccommodationRepository : IRepository<Accommodation>
    {
        public readonly DatabaseContext _databaseContext = ProvideDependencyFactory.databaseContext;

        public void Create(Accommodation accommodation)
        {
            _databaseContext.Address.Add(accommodation.Address);
            _databaseContext.Accommodation.Add(accommodation);
            _databaseContext.SaveChanges();
        }

        public void Delete(int id)
        {
            var acc = _databaseContext.Accommodation.Find(id);
            _databaseContext.Address.Remove(acc.Address);
            _databaseContext.Accommodation.Remove(Read(id));
            _databaseContext.SaveChanges();
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
            _databaseContext.SaveChanges();
        }
    }
}
