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

        public BookingInfoPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click_Get(object sender, RoutedEventArgs e)
        {
            try
            {
                await ViewModel.GetABooking();
                listView.ItemsSource = ViewModel.Booking.Dates;
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
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
    }
}
