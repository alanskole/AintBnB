using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AintBnB.Core.Models
{
    public class Accommodation : INotifyPropertyChanged
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
        private List<byte[]> _picture;
        private double _averageRating;
        private int _amountOfRatings;

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                NotifyPropertyChanged("Id");
            }
        }

        public User Owner
        {
            get { return _owner; }
            set
            {
                _owner = value;
                NotifyPropertyChanged("Owner");
            }
        }

        public Address Address
        {
            get { return _address; }

            set
            {
                _address = value;
                NotifyPropertyChanged("Address");
            }
        }

        public int SquareMeters
        {
            get { return _squareMeters; }
            set
            {
                _squareMeters = value;
                NotifyPropertyChanged("SquareMeters");
            }
        }

        public int AmountOfBedrooms
        {
            get { return _amountOfBedrooms; }
            set
            {
                _amountOfBedrooms = value;
                NotifyPropertyChanged("AmountOfBedrooms");
            }
        }

        public double KilometersFromCenter
        {
            get { return _kilometersFromCenter; }
            set
            {
                _kilometersFromCenter = Math.Floor(value * 10) / 10;
                NotifyPropertyChanged("KilometersFromCenter");
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                NotifyPropertyChanged("Description");
            }
        }

        public int PricePerNight
        {
            get { return _pricePerNight; }
            set
            {
                _pricePerNight = value;
                NotifyPropertyChanged("PricePerNight");
            }
        }

        public int CancellationDeadlineInDays
        {
            get { return _cancellationDeadlineInDays; }
            set
            {
                _cancellationDeadlineInDays = value;
                NotifyPropertyChanged("CancellationDeadlineInDays");
            }
        }

        public SortedDictionary<string, bool> Schedule
        {
            get { return _schedule; }
            set
            {
                _schedule = value;
                NotifyPropertyChanged("Schedule");
            }
        }

        public List<byte[]> Picture
        {
            get { return _picture; }
            set
            {
                _picture = value;
                NotifyPropertyChanged("Picture");
            }
        }

        public double AverageRating
        {
            get { return _averageRating; }
            set
            {
                _averageRating = Math.Floor(value * 10) / 10;
                NotifyPropertyChanged("AverageRating");
            }
        }

        public int AmountOfRatings
        {
            get { return _amountOfRatings; }
            set
            {
                _amountOfRatings = value;
                NotifyPropertyChanged("AmountOfRatings");
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

        public Accommodation()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
