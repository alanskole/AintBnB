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
                _id = value;
                NotifyPropertyChanged("Id");
            }
        }

        public string Street
        {
            get { return _street; }

            set
            {
                _street = value;
                NotifyPropertyChanged("Street");
            }
        }

        public int Number
        {
            get { return _number; }

            set
            {
                _number = value;
                NotifyPropertyChanged("Number");
            }
        }

        public int Zip
        {
            get { return _zip; }

            set
            {
                _zip = value;
                NotifyPropertyChanged("Zip");
            }
        }

        public string Area
        {
            get { return _area; }

            set
            {
                _area = value;
                NotifyPropertyChanged("Area");
            }
        }

        public string City
        {
            get { return _city; }

            set
            {
                _city = value;
                NotifyPropertyChanged("City");
            }
        }

        public string Country 
        {
            get { return _country; }
            set
            {
                _country = value;
                NotifyPropertyChanged("Country");
            }
        }

        public override string ToString()
        {
            return ($"Address: {Street} {Number}, {Zip} {Area}, {City}, {Country}");
        }

        public Address(string street, int number, int zip, string area, string city, string country)
        {
            Street = street.Trim();
            Number = number;
            Zip = zip;
            Area = area.Trim();
            City = city.Trim();
            Country = country.Trim();
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