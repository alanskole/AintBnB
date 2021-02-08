using AintBnB.Core.Models;
using AintBnB.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AintBnB.Views
{
    public sealed partial class AllBookingsOfOwnedAccommodations : Page
    {
        public BookingViewModel ViewModel { get; } = new BookingViewModel();

        public AllBookingsOfOwnedAccommodations()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                listView.ItemsSource = await ViewModel.GetAllBookingsOfOwnedAccommodations();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = listView.SelectedIndex;

            List<Booking> allBookings = await ViewModel.GetAllBookings();

            Booking booking = allBookings[index];

            var container = new StackPanel();

            var contentDialog = new ContentDialog
            {
                Title = "Delete Booking",
                Content = container,
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await contentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var dialog = new MessageDialog("This will delete the booking! Are you sure?");
                dialog.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                dialog.Commands.Add(new UICommand { Label = "Cancel", Id = 1 });
                var res = await dialog.ShowAsync();

                try
                {
                    if ((int)res.Id == 0)
                    {
                        ViewModel.Booking.Id = booking.Id;
                        await ViewModel.DeleteABooking();
                        await new MessageDialog("Deletion successful!").ShowAsync();
                    }
                }
                catch (Exception ex)
                {
                    await new MessageDialog(ex.Message).ShowAsync();
                }
            }
        }
    }
}
