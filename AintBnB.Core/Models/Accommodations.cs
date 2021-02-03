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
        private SortedDictionary<string, bool> _schedule;

        public int Id
        {
            get { return _id; }
            set
            {
                if (value == 0)
                    throw new ArgumentException("Id name cannot be zero");
                _id = value;
                NotifyPropertyChanged("Id");
            }
        }

        public User Owner
        {
            get { return _owner; }
            set
            {
                if (value == null)
                    throw new ArgumentException("Owner name cannot be null");
                _owner = value;
                NotifyPropertyChanged("Owner");
            }
        }

        public Address Address
        {
            get { return _address; }

            set
            {
                if (value == null)
                    throw new ArgumentException("Address cannot be null");
                _address = value;
                NotifyPropertyChanged("Address");
            }
        }

        public int SquareMeters
        {
            get { return _squareMeters; }
            set
            {
                if (value == 0)
                    throw new ArgumentException("Square meters cannot be zero");
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
                if (value == null)
                    throw new ArgumentException("Description cannot be null");
                _description = value;
                NotifyPropertyChanged("Description");
            }
        }

        public int PricePerNight
        {
            get { return _pricePerNight; }
            set
            {
                if (value == 0)
                    throw new ArgumentException("Price per night cannot be zero");
                _pricePerNight = value;
                NotifyPropertyChanged("PricePerNight");
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

        public Accommodation(User owner, Address address, int squareMeters, int amountOfBedroooms, double kilometersFromCenter, string description, int pricePerNight)
        {
            Owner = owner;
            Address = address;
            SquareMeters = squareMeters;
            AmountOfBedrooms = amountOfBedroooms;
            KilometersFromCenter = kilometersFromCenter;
            Description = description;
            PricePerNight = pricePerNight;
        }

        /*public Accommodation(int squareMeters, int amountOfBedroooms, double kilometersFromCenter, string description, int pricePerNight)
        {
            _squareMeters = squareMeters;
            _amountOfBedrooms = amountOfBedroooms;
            _kilometersFromCenter = kilometersFromCenter;
            _description = description;
            _pricePerNight = pricePerNight;
        }*/

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
