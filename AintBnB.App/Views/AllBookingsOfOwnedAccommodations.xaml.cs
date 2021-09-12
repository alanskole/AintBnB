using AintBnB.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AintBnB.App.Helpers.CommonViewMethods;

namespace AintBnB.App.Views
{
    public sealed partial class AllBookingsOfOwnedAccommodations : Page
    {
        public BookingViewModel BookingViewModel { get; } = new BookingViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();
        public UserViewModel UserViewModel { get; } = new UserViewModel();

        private bool _skipSelectionChanged;

        public AllBookingsOfOwnedAccommodations()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await AuthenticationViewModel.IsAdminAsync();

                await FillComboboxWithIdsOfAllTheCustomersAsync();
            }
            catch (Exception)
            {
                await FillListWithBookingsAsync(AuthenticationViewModel.IdOfLoggedInUser);
            }
        }

        private async Task FillComboboxWithIdsOfAllTheCustomersAsync()
        {
            var ids = new List<int>();

            await UserViewModel.GetAllCustomersAsync();

            foreach (var user in UserViewModel.AllUsers)
                ids.Add(user.Id);

            ComboBoxUsers.ItemsSource = ids;

            ComboBoxUsers.Visibility = Visibility.Visible;
        }

        private async void ComboBoxUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                await FillListWithBookingsAsync(int.Parse(ComboBoxUsers.SelectedValue.ToString()));

            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async Task FillListWithBookingsAsync(int userId)
        {
            BookingViewModel.UserId = userId;

            try
            {
                await BookingViewModel.GetAllBookingsOfOwnedAccommodationsAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_skipSelectionChanged)
            {
                _skipSelectionChanged = false;

                return;
            }

            BookingViewModel.Booking.Id = BookingViewModel.AllBookings[listView.SelectedIndex].Id;

            contentDialog.Visibility = Visibility.Visible;

            ContentDialogResult result = await contentDialog.ShowAsync();

            _skipSelectionChanged = true;
            listView.SelectedItem = null;

            if (result == ContentDialogResult.Primary)
            {
                var res = await DialogeMessageAsync("This will delete the booking of your accommodation! Are you sure?", "Delete");

                if ((int)res.Id == 1)
                    return;

                await DeleteBookingAsync();
            }
        }

        private async Task DeleteBookingAsync()
        {
            try
            {
                await BookingViewModel.DeleteABookingAsync();

                await new MessageDialog("Booking deleted!").ShowAsync();


                Frame.Navigate(typeof(AllBookingsOfOwnedAccommodations));
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }
    }
}
