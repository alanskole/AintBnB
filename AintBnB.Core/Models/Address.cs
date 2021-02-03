using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AintBnB.Core.Models
{
    public class Address : INotifyPropertyChanged
    {
        private int _id;
        private string _street;
        private int _number;
        private int _zip;
        private string _area;
        private string _city;
        private string _country;
        public int Id
        {
            get { return _id; }

            set
            {
                if (value == 0)
                    throw new ArgumentException("Id cannot be zero");
                _id = value;
                NotifyPropertyChanged("Id");
            }
        }

        public string Street
        {
            get { return _street; }

            set
            {
                if (value == null)
                    throw new ArgumentException("Street cannot be null");
                _street = value;
                NotifyPropertyChanged("Street");
            }
        }

        public int Number
        {
            get { return _number; }

            set
            {
                if (value == 0)
                    throw new ArgumentException("Number cannot be zero");
                _number = value;
                NotifyPropertyChanged("Number");
            }
        }

        public int Zip
        {
            get { return _zip; }

            set
            {
                if (value == 0)
                    throw new ArgumentException("Zip cannot be zero");
                _zip = value;
                NotifyPropertyChanged("Zip");
            }
        }

        public string Area
        {
            get { return _area; }

            set
            {
                if (value == null)
                    throw new ArgumentException("Area cannot be null");
                _area = value;
                NotifyPropertyChanged("Area");
            }
        }

        public string City
        {
            get { return _city; }

            set
            {
                if (value == null)
                    throw new ArgumentException("City cannot be null");
                _city = value;
                NotifyPropertyChanged("City");
            }
        }

        public string Country 
        {
            get { return _country; }
            set
            {
                if (value == null)
                    throw new ArgumentException("Country cannot be null");
                _country = value;
                NotifyPropertyChanged("Country");
            }
        }

        public Address(string street, int number, int zip, string area, string city, string country)
        {
            Street = street;
            Number = number;
            Zip = zip;
            Area = area;
            City = city;
            Country = country;
        }

        public Address()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}