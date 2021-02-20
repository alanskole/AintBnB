using AintBnB.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AintBnB.CommonMethodsAndProperties.CommonViewMethods;

namespace AintBnB.Views
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
            await CheckIfAnyoneIsLoggedIn();

            try
            {
                await AuthenticationViewModel.IsEmployeeOrAdmin();

                await FillComboboxWithIdsOfAllTheCustomers();
            }
            catch (Exception)
            {
                await FillListWithBookings(await AuthenticationViewModel.IdOfLoggedInUser());
            }
        }

        private async Task CheckIfAnyoneIsLoggedIn()
        {
            try
            {
                await AuthenticationViewModel.IsAnyoneLoggedIn();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async Task FillComboboxWithIdsOfAllTheCustomers()
        {
            List<int> ids = new List<int>();

            foreach (var user in await UserViewModel.GetAllCustomers())
                ids.Add(user.Id);

            ComboBoxUsers.ItemsSource = ids;

            ComboBoxUsers.Visibility = Visibility.Visible;
        }

        private async void ComboBoxUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                await FillListWithBookings(int.Parse(ComboBoxUsers.SelectedValue.ToString()));

            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async Task FillListWithBookings(int userId)
        {
            BookingViewModel.UserId = userId;

            try
            {
                listView.ItemsSource = await BookingViewModel.GetAllBookingsOfOwnedAccommodations();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_skipSelectionChanged)
            {
                _skipSelectionChanged = false;

                return;
            }

            BookingViewModel.Booking.Id = BookingViewModel.AllBookingsOfOwnedAccommodations[listView.SelectedIndex].Id;

            var container = new StackPanel();

            var contentDialog = new ContentDialog
            {
                Title = "Delete Booking",
                Content = container,
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await contentDialog.ShowAsync();

            _skipSelectionChanged = true;
            listView.SelectedItem = null;

            if (result == ContentDialogResult.Primary)
            {
                var res = await DialogeMessageAsync("This will delete the booking of your accommodation! Are you sure?", "Delete");

                if ((int)res.Id == 1)
                    return;

                await DeleteBooking();
            }
        }

        private async Task DeleteBooking()
        {
            try
            {
                await BookingViewModel.DeleteABooking();

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
