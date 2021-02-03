using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AintBnB.Core.Models
{
    public class Booking : INotifyPropertyChanged
    {
        private int _id;
        private User _bookedBy;
        private Accommodation _accommodation;
        private List<string> _dates;
        private int _price;

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

        public User BookedBy
        {
            get { return _bookedBy; }
            set
            {
                if (value == null)
                    throw new ArgumentException("Booked by cannot be null");
                _bookedBy = value;
                NotifyPropertyChanged("BookedBy");
            }
        }

        public Accommodation Accommodation
        {
            get { return _accommodation; }
            set
            {
                if (value == null)
                    throw new ArgumentException("Accommodation cannot be null");
                _accommodation = value;
                NotifyPropertyChanged("Accommodation");
            }
        }

        public List<string> Dates
        {
            get { return _dates; }
            set
            {
                if (value == null)
                    throw new ArgumentException("Dates cannot be null");
                _dates = value;
                NotifyPropertyChanged("Dates");
            }
        }

        public int Price
        {
            get { return _price; }
            set
            {
                if (value == 0)
                    throw new ArgumentException("Price name cannot be zero");
                _price = value;
                NotifyPropertyChanged("Price");
            }
        }

        public Booking(User bookedBy, Accommodation accommodation, List<string> dates, int price)
        {
            BookedBy = bookedBy;
            Accommodation = accommodation;
            Dates = dates;
            Price = price;
        }

        public Booking()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}