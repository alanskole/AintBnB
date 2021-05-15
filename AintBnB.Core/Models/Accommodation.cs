using System;
using System.Collections.Generic;
using System.Net;

namespace AintBnB.Core.Models
{
    public class Accommodation
    {
        private int _id;
        private User _owner;
        private Address _address;
        private int _squareMeters;
        private int _amountOfBedrooms;
        private double _kilometersFromCenter;
        private string _description;
        private int _pricePerNight;
        private int _cancellationDeadlineInDays;
        private SortedDictionary<string, bool> _schedule;
        private double _averageRating;
        private int _amountOfRatings;

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
            }
        }

        public User Owner
        {
            get { return _owner; }
            set
            {
                _owner = value;
            }
        }

        public Address Address
        {
            get { return _address; }

            set
            {
                _address = value;
            }
        }

        public int SquareMeters
        {
            get { return _squareMeters; }
            set
            {
                _squareMeters = value;
            }
        }

        public int AmountOfBedrooms
        {
            get { return _amountOfBedrooms; }
            set
            {
                _amountOfBedrooms = value;
            }
        }

        public double KilometersFromCenter
        {
            get { return _kilometersFromCenter; }
            set
            {
                _kilometersFromCenter = Math.Floor(value * 10) / 10;
            }
        }

        public string Description
        {
            get { return WebUtility.HtmlDecode(_description); }
            set
            {
                _description = WebUtility.HtmlEncode(value);
            }
        }

        public int PricePerNight
        {
            get { return _pricePerNight; }
            set
            {
                _pricePerNight = value;
            }
        }

        public int CancellationDeadlineInDays
        {
            get { return _cancellationDeadlineInDays; }
            set
            {
                _cancellationDeadlineInDays = value;
            }
        }

        public SortedDictionary<string, bool> Schedule
        {
            get { return _schedule; }
            set
            {
                _schedule = value;
            }
        }

        public double AverageRating
        {
            get { return _averageRating; }
            set
            {
                _averageRating = Math.Floor(value * 10) / 10;
            }
        }

        public int AmountOfRatings
        {
            get { return _amountOfRatings; }
            set
            {
                _amountOfRatings = value;
            }
        }

        public override string ToString()
        {
            return
                ($"Accommodation ID: {Id} " +
                $"\nOwner: {Owner} " +
                $"\nAddress: {Address} " +
                $"\nSquare meters: {SquareMeters} " +
                $"\nBedrooms: {AmountOfBedrooms} " +
                $"\nKilometers from center: {KilometersFromCenter} " +
                $"\nNightly price: {PricePerNight} " +
                $"\nCancellation deadline in days: {CancellationDeadlineInDays}" +
                $"\nAverage rating: {AverageRating} based on {AmountOfRatings} ratings" +
                $"\nDescription: {Description}");
        }

        public Accommodation()
        {

        }

        public Accommodation(User owner, Address address, int squareMeters, int amountOfBedroooms, double kilometersFromCenter, string description, int pricePerNight, int cancellationDeadlineInDays)
        {
            Owner = owner;
            Address = address;
            SquareMeters = squareMeters;
            AmountOfBedrooms = amountOfBedroooms;
            KilometersFromCenter = kilometersFromCenter;
            Description = description;
            PricePerNight = pricePerNight;
            CancellationDeadlineInDays = cancellationDeadlineInDays;
        }
    }
}
