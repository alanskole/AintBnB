using AintBnB.App.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace AintBnB.App.Views
{
    public sealed partial class AllBookingsPage : Page
    {
        public BookingViewModel BookingViewModel { get; } = new BookingViewModel();

        public AllBookingsPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                listView.ItemsSource = await BookingViewModel.GetAllBookingsAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Frame.Navigate(typeof(BookingInfoPage), listView.SelectedIndex);
        }
    }
}
