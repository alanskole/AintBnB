using AintBnB.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using static AintBnB.CommonMethodsAndProperties.CommonViewMethods;

namespace AintBnB.Views
{
    public sealed partial class BookingInfoPage : Page
    {
        public BookingViewModel BookingViewModel { get; } = new BookingViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();
        public BookingInfoPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                BookingViewModel.UserId = await AuthenticationViewModel.IdOfLoggedInUser();

                List<int> ids = new List<int>();

                foreach (var booking in await BookingViewModel.GetAllBookings())
                    ids.Add(booking.Id);

                ComboBoxBookings.ItemsSource = ids;
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            WhenNavigatedToView(e, ComboBoxBookings);
        }

        private async void Button_Click_Update(object sender, RoutedEventArgs e)
        {
            contentDialog.Visibility = Visibility.Visible;

            ContentDialogResult result = await contentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var res = await DialogeMessageAsync("Are you sure you want to change the booking dates?", "Change");

                if ((int)res.Id == 1)
                    return;

                try
                {
                    await BookingViewModel.UpdateBooking();
                    await new MessageDialog("Update ok!").ShowAsync();
                    Frame.Navigate(typeof(AllBookingsPage));
                }
                catch (Exception ex)
                {
                    await new MessageDialog(ex.Message).ShowAsync();
                }
            }
        }

        private void MyDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            BookingViewModel.StartDate = DatePickerParser(MyDatePicker);
        }

        private async void Button_Click_Rate(object sender, RoutedEventArgs e)
        {
            int rating = (int)ComboBoxRating.SelectedItem;

            var res = await DialogeMessageAsync($"Ratings cannot be changed, are you sure you want to rate it {rating}?", "Rate");

            if ((int)res.Id == 1)
                return;

            try
            {
                BookingViewModel.Booking.Rating = rating;
                await BookingViewModel.Rate();
                Frame.Navigate(typeof(AllBookingsPage));
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

            try
            {
                await BookingViewModel.DeleteABooking();
                await new MessageDialog("Deletion ok!").ShowAsync();
                Frame.Navigate(typeof(BookingInfoPage));
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

                await BookingViewModel.GetABooking();

                listView.ItemsSource = BookingViewModel.Booking.Dates;
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }

            if (BookingViewModel.Booking.Rating > 0)
            {
                Rating.Visibility = Visibility.Visible;
                ComboBoxRating.Visibility = Visibility.Collapsed;
                RateButton.Visibility = Visibility.Collapsed;
            }
        }
    }
}
