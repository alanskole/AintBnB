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

        public SortedDictionary<string, bool> Schedule
        {
            get { return _schedule; }
            set
            {
                _schedule = value;
                NotifyPropertyChanged("Schedule");
            }
        }

        public override string ToString()
        {
            return ($"ID: {Id}. Owner's name {Owner.FirstName} {Owner.LastName}. {Address}. Square meters {SquareMeters}. " +
                $"Bedrooms {AmountOfBedrooms}. Kilometers from center {KilometersFromCenter} Nightly price {PricePerNight} \n{Description}");
        }

        public Accommodation(User owner, Address address, int squareMeters, int amountOfBedroooms, double kilometersFromCenter, string description, int pricePerNight)
        {
            Owner = owner;
            Address = address;
            SquareMeters = squareMeters;
            AmountOfBedrooms = amountOfBedroooms;
            KilometersFromCenter = kilometersFromCenter;
            Description = description.Trim();
            PricePerNight = pricePerNight;
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
