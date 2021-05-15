using AintBnB.Core.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AintBnB.Database.DbCtx
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Accommodation> Accommodation { get; set; }
        public DbSet<Address> Address { get; set; }
        public DbSet<Booking> Booking { get; set; }
        public DbSet<Image> Image { get; set; }
        public DbSet<User> User { get; set; }
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(e => e.UserType)
                .HasConversion(
                    v => v.ToString(),
                    v => (UserTypes)Enum.Parse(typeof(UserTypes), v));

            modelBuilder.Entity<Booking>()
                .Property(b => b.Dates)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<string>>(v));

            modelBuilder.Entity<Accommodation>()
                .Property(b => b.Schedule)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<SortedDictionary<string, bool>>(v));

            modelBuilder.Entity<Accommodation>()
                .HasMany(a => a.Picture)
                .WithOne(a => a.Accommodation);


            modelBuilder.Entity<Accommodation>()
                .HasOne(b => b.Owner)
                .WithMany()
                .HasForeignKey("OwnerId")
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Accommodation>()
                .HasOne(b => b.Address)
                .WithMany()
                .HasForeignKey("AddressId")
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.BookedBy)
                .WithMany()
                .HasForeignKey("BookedById")
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Accommodation)
                .WithMany()
                .HasForeignKey("AccommodationId")
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}