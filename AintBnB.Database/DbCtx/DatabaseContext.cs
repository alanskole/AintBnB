using AintBnB.Core.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AintBnB.Database.DbCtx
{
    public class DatabaseContext  : DbContext
    {
        public DbSet<Accommodation> Accommodation { get; set; }
        public DbSet<Address> Address { get; set; }
        public DbSet<Booking> Booking { get; set; }
        public DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = "(localdb)\\MSSQLLocalDB",
                InitialCatalog = "AintBnB.Database",
                IntegratedSecurity = true
            };

            optionsBuilder.UseSqlServer(builder.ConnectionString.ToString());
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
                .Property(b => b.Picture)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<byte[]>>(v));


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