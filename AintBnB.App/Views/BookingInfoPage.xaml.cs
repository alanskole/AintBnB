﻿using AintBnB.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using static AintBnB.App.Helpers.CommonViewMethods;

namespace AintBnB.App.Views
{
    public sealed partial class BookingInfoPage : Page
    {
        public BookingViewModel BookingViewModel { get; } = new BookingViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();
        private int _selectedIndex = -1;

        public BookingInfoPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _selectedIndex = WhenNavigatedToView(e, ComboBoxBookings);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await AuthenticationViewModel.LoggedInUserIdAndUserType();
                BookingViewModel.UserId = AuthenticationViewModel.IdOfLoggedInUser;

                await FillComboboxWithAllBookingsAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async Task FillComboboxWithAllBookingsAsync()
        {
            var ids = new List<int>();

            await BookingViewModel.GetAllBookingsAsync();

            foreach (var booking in BookingViewModel.AllBookings)
                ids.Add(booking.Id);

            ComboBoxBookings.ItemsSource = ids;
        }

        private async void Button_Click_Update(object sender, RoutedEventArgs e)
        {
            contentDialog.Visibility = Visibility.Visible;

            var result = await contentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var res = await DialogeMessageAsync("Are you sure you want to change the booking dates?", "Change");

                if ((int)res.Id == 1)
                    return;

                await UpdateTheBookingAsync();
            }
        }

        private async Task UpdateTheBookingAsync()
        {
            try
            {
                await BookingViewModel.UpdateBookingAsync();
                await new MessageDialog("Update ok!").ShowAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private void MyDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            BookingViewModel.StartDate = DatePickerParser(MyDatePicker);
        }

        private async void Button_Click_Rate(object sender, RoutedEventArgs e)
        {
            var rating = (int)ComboBoxRating.SelectedItem;

            var res = await DialogeMessageAsync($"Ratings cannot be changed, are you sure you want to rate it {rating}?", "Rate");

            if ((int)res.Id == 1)
                return;

            await RateAsync(rating);
        }

        private async Task RateAsync(int rating)
        {
            try
            {
                BookingViewModel.Booking.Rating = rating;
                await BookingViewModel.RateAsync();
                RatingButtonVisibility();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void Button_Click_Delete(object sender, RoutedEventArgs e)
        {
            var res = await DialogeMessageAsync("This will delete the booking! Are you sure?", "Delete");

            if ((int)res.Id == 1)
                return;

            await DeleteBookingAsync();
        }

        private async Task DeleteBookingAsync()
        {
            try
            {
                await BookingViewModel.DeleteABookingAsync();
                await new MessageDialog("Deletion ok!").ShowAsync();

                Frame.Navigate(typeof(AllBookingsPage));
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void ComboBoxBookings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                BookingViewModel.Booking.Id = int.Parse(ComboBoxBookings.SelectedValue.ToString());

                await BookingViewModel.GetABookingAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }

            HandleButtonVisibility();
        }

        private void HandleButtonVisibility()
        {
            ShowButtons();
            RatingButtonVisibility();
        }

        private void RatingButtonVisibility()
        {
            if (BookingViewModel.Booking.Rating > 0)
            {
                Rating.Visibility = Visibility.Visible;
                ComboBoxRating.Visibility = Visibility.Collapsed;
                RateButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                Rating.Visibility = Visibility.Collapsed;
                ComboBoxRating.Visibility = Visibility.Visible;
                RateButton.Visibility = Visibility.Visible;
            }
        }

        private void ShowButtons()
        {
            bookedById.Visibility = Visibility.Visible;
            bookedByUsername.Visibility = Visibility.Visible;
            bookedByFirstname.Visibility = Visibility.Visible;
            bookedByLastname.Visibility = Visibility.Visible;
            streetOfAccommodation.Visibility = Visibility.Visible;
            numberOfAccommodation.Visibility = Visibility.Visible;
            zipOfAccommodation.Visibility = Visibility.Visible;
            areaOfAccommodation.Visibility = Visibility.Visible;
            cityOfAccommodation.Visibility = Visibility.Visible;
            countryOfAccommodation.Visibility = Visibility.Visible;
            priceOfBooking.Visibility = Visibility.Visible;
            cancellationDeadline.Visibility = Visibility.Visible;
            updateButton.Visibility = Visibility.Visible;
            deleteButton.Visibility = Visibility.Visible;
            ComboBoxRating.Visibility = Visibility.Visible;
            RateButton.Visibility = Visibility.Visible;
            datesBooked.Visibility = Visibility.Visible;
        }
    }
}
