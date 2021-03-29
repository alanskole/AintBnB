using AintBnB.Core.Models;
using AintBnB.Database.DbCtx;
using AintBnB.BusinessLogic.DependencyProviderFactory;
using System.Collections.Generic;
using static AintBnB.BusinessLogic.Services.CountryNameFormatter;
using static AintBnB.BusinessLogic.Services.AllCountiresAndCitiesEurope;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;

namespace AintBnB.BusinessLogic.Repository
{
    public class AccommodationRepository : IRepository<Accommodation>
    {
        public readonly DatabaseContext _databaseContext = ProvideDependencyFactory.databaseContext;

        public void Create(Accommodation accommodation)
        {
            if (!AllEuropeanCities().ContainsKey(FormatName(accommodation.Address.Country)))
                throw new ArgumentException("Country not valid");
            if (!europe[FormatName(accommodation.Address.Country)].Any(city => city.Equals(accommodation.Address.City, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("City not valid");

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
            if (!AllEuropeanCities().ContainsKey(FormatName(accommodation.Address.Country)))
                throw new ArgumentException("Country not valid");
            if (!europe[FormatName(accommodation.Address.Country)].Any(city => city.Equals(accommodation.Address.City, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("City not valid");



            var acc = _databaseContext.Accommodation.Find(id);
            acc.Address.Street = accommodation.Address.Street;
            acc.Address.Number = accommodation.Address.Number;
            acc.Address.Zip = accommodation.Address.Zip;
            acc.Address.Area = accommodation.Address.Area;
            acc.Address.City = accommodation.Address.City;
            acc.Address.Country = accommodation.Address.Country;
            acc.SquareMeters = accommodation.SquareMeters;
            acc.AmountOfBedrooms = accommodation.AmountOfBedrooms;
            acc.KilometersFromCenter = accommodation.KilometersFromCenter;
            acc.Description = accommodation.Description;
            acc.PricePerNight = accommodation.PricePerNight;
            _databaseContext.SaveChanges();
        }
    }
}
