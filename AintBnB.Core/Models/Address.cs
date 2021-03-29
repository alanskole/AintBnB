using System;

namespace AintBnB.Core.Models
{
    public class Address
    {
        private int _id;
        private string _street;
        private string _number;
        private string _zip;
        private string _area;
        private string _city;
        private string _country;

        public int Id
        {
            get { return _id; }

            set
            {
                _id = value;
            }
        }

        public string Street
        {
            get { return _street; }

            set
            {
                _street = value;
            }
        }

        public string Number
        {
            get { return _number; }

            set
            {
                _number = value;
            }
        }

        public string Zip
        {
            get { return _zip; }

            set
            {
                _zip = value;
            }
        }

        public string Area
        {
            get { return _area; }

            set
            {
                _area = value;
            }
        }

        public string City
        {
            get { return _city; }

            set
            {
                _city = value;
            }
        }

        public string Country 
        {
            get { return _country; }
            set
            {
                _country = value;
            }
        }

        public override string ToString()
        {
            return ($"Address: {Street} {Number}, {Zip} {Area}, {City}, {Country}");
        }

        public Address(string street, string number, string zip, string area, string city, string country)
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
    }
}