using AintBnB.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AintBnB.Views
{
    public sealed partial class AllBookingsOfOwnedAccommodations : Page
    {
        public BookingViewModel BookingViewModel { get; } = new BookingViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();
        private bool _skipSelectionChanged;

        public AllBookingsOfOwnedAccommodations()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                BookingViewModel.UserId = await AuthenticationViewModel.IdOfLoggedInUser();

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
                var dialog = new MessageDialog("This will delete the booking of your accommodation! Are you sure?");
                dialog.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                dialog.Commands.Add(new UICommand { Label = "Cancel", Id = 1 });
                var res = await dialog.ShowAsync();

                if ((int)res.Id == 0)
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
    }
}
