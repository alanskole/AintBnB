using AintBnB.Core.Models;
using AintBnB.BusinessLogic.DependencyProviderFactory;
using AintBnB.BusinessLogic.Repository;
using static AintBnB.BusinessLogic.Services.DateParser;
using static AintBnB.BusinessLogic.Services.UpdateScheduleInDatabase;
using System;
using System.Collections.Generic;

namespace AintBnB.BusinessLogic.Services
{
    public class BookingService : IBookingService
    {
        private IRepository<Booking> _iBookingRepository;

        public IRepository<Booking> IBookingRepository
        {
            get { return _iBookingRepository; }
            set
            {
                if (value == null)
                    throw new ArgumentException("IBookingRepository cannot be null");
                _iBookingRepository = value;
            }
        }

        public BookingService()
        {
            _iBookingRepository = ProvideDependencyFactory.bookingRepository;
        }

        public BookingService(IRepository<Booking> bookingRepo)
        {
            _iBookingRepository = bookingRepo;

        }

        public Booking GetBooking(int id)
        {
            return _iBookingRepository.Read(id);
        }

        public List<Booking> GetAllBookings()
        {
            return _iBookingRepository.GetAll();
        }

        public Booking Book(string startDate, User booker, int nights, Accommodation accommodation)
        {
            if (nights < 1)
                throw new ArgumentException("Must book for at least a night");
            
            startDate = startDate.Trim();

            if (AreAllDatesAvailable(accommodation.Schedule, startDate, nights))
            {
                List<string> datesBooked = new List<string>();
                AddDatesToList(datesBooked, startDate, nights);
                SetStatusToUnavailable(accommodation, datesBooked);
                int totalPrice = nights * accommodation.PricePerNight;
                Booking booking = new Booking(booker, accommodation, datesBooked, totalPrice);
                _iBookingRepository.Create(booking);
                return booking;
            }
            else
                throw new ArgumentException("Dates aren't available");
        }

        private void AddDatesToList(List<string> datesBooked, string startDate, int nights)
        {
            for (int i = 0; i < nights; i++)
            {
                string dateToAdd = DateFormatterCustomDate(DateTime.Parse(startDate).AddDays(i));
                datesBooked.Add(dateToAdd);
            }
        }

        private void SetStatusToUnavailable(Accommodation accommodation, List<string> datesBooked)
        {
            for (int i = 0; i < datesBooked.Count; i++)
            {
                accommodation.Schedule[datesBooked[i]] = false;
            }

            UpdateScheduleInDb(accommodation.Id, accommodation.Schedule);
        }
    }
}
