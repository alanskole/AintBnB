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

        private void Button_Click_Update(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BookingUpdatePage), BookingViewModel.Booking.Id);
        }

        private async void Button_Click_Delete(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("This will delete the booking! Are you sure?");
            dialog.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
            dialog.Commands.Add(new UICommand { Label = "Cancel", Id = 1 });
            var res = await dialog.ShowAsync();

            if ((int)res.Id == 0)
            {
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
        }
    }
}
