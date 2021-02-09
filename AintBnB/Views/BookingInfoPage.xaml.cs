using AintBnB.Core.Models;
using AintBnB.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AintBnB.Views
{
    public sealed partial class BookingInfoPage : Page
    {
        public BookingViewModel ViewModel { get; } = new BookingViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();

        public BookingInfoPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click_Delete(object sender, RoutedEventArgs e)
        {
            try
            {
                await ViewModel.DeleteABooking();
                await new MessageDialog("Deletion ok!").ShowAsync();
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
                ViewModel.Booking.Id = int.Parse(ComboBoxBookings.SelectedValue.ToString());

                await ViewModel.GetABooking();

                listView.ItemsSource = ViewModel.Booking.Dates;
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.UserId = await AuthenticationViewModel.IdOfLoggedInUser();

                List<int> ids = new List<int>();

                foreach (var booking in await ViewModel.GetAllBookings())
                    ids.Add(booking.Id);

                ComboBoxBookings.ItemsSource = ids;
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }
    }
}
