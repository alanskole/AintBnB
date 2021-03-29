using System;
using System.Collections.Generic;

namespace AintBnB.Core.Models
{
    public class Booking
    {
        private int _id;
        private User _bookedBy;
        private Accommodation _accommodation;
        private List<string> _dates;
        private int _price;
        private int _rating;

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
            }
        }

        public User BookedBy
        {
            get { return _bookedBy; }
            set
            {
                _bookedBy = value;
            }
        }

        public Accommodation Accommodation
        {
            get { return _accommodation; }
            set
            {
                _accommodation = value;
            }
        }

        public List<string> Dates
        {
            get { return _dates; }
            set
            {
                _dates = value;
            }
        }

        public int Price
        {
            get { return _price; }
            set
            {
                _price = value;
            }
        }

        public int Rating
        {
            get { return _rating; }
            set
            {
                _rating = value;
            }
        }

        public override string ToString()
        {
            return ($"Booking ID: {Id} \nBooked by: {BookedBy}. \nAccommodation: {Accommodation}. \nDates Booked: {string.Join(", ", Dates.ToArray())}. \nTotal price: {Price} {(Rating < 1 ? "" : $"\nRating given by the booker: {Rating}")}");
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
    }
}