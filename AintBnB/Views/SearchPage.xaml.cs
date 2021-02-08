using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using AintBnB.ViewModels;
using AintBnB.Core.Models;
using Windows.UI.Popups;

namespace AintBnB.Views
{
    public sealed partial class SearchPage : Page
    {
        public AccommodationViewModel ViewModel { get; } = new AccommodationViewModel();
        public EuropeViewModel EuropeViewModel { get; } = new EuropeViewModel();
        public BookingViewModel BookingViewModel { get; } = new BookingViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();

        public SearchPage()
        {
            this.InitializeComponent();
        }

        private async void ComboBoxCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EuropeViewModel.Country = ComboBoxCountries.SelectedValue.ToString();

            ComboBoxCities.ItemsSource = await EuropeViewModel.GetAllCitiesOfACountry();
        }

        private void ComboBoxCities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EuropeViewModel.City = ComboBoxCities.SelectedValue.ToString();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBoxCountries.ItemsSource = await EuropeViewModel.GetAllCountriesInEurope();
        }

        private async void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = listView.SelectedIndex;

            List<Accommodation> availableAccs = await ViewModel.GetAllAccommodations();

            Accommodation acc = availableAccs[index];

            var container = new StackPanel();

            var contentDialog = new ContentDialog
            {
                Title = "Book Accommodation",
                Content = container,
                PrimaryButtonText = "Book",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await contentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                BookingViewModel.StartDate = ViewModel.FromDate;
                BookingViewModel.Booking.BookedBy.Id = await AuthenticationViewModel.IdOfLoggedInUser();
                BookingViewModel.Nights = int.Parse(nights.Text);
                BookingViewModel.Booking.Accommodation.Id = acc.Id;
                try
                {
                    await BookingViewModel.BookAccommodation();
                    await new MessageDialog("Booking successful!").ShowAsync();
                }
                catch (Exception ex)
                {
                    await new MessageDialog(ex.Message).ShowAsync();
                }
            }

        }

        private async void Button_Click_Search(object sender, RoutedEventArgs e)
        {
            ViewModel.Accommodation.Address.Country = EuropeViewModel.Country;

            ViewModel.Accommodation.Address.City = EuropeViewModel.City;

            try
            {
                listView.ItemsSource = await ViewModel.GetAvailable();

            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private void MyDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            var date = MyDatePicker.Date;

            DateTime dt = date.Value.DateTime;

            ViewModel.FromDate = dt.ToString("yyyy-MM-dd");
        }
    }
}
