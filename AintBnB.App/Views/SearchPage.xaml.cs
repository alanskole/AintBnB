using AintBnB.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AintBnB.App.CommonMethodsAndProperties.CommonViewMethods;

namespace AintBnB.App.Views
{
    public sealed partial class SearchPage : Page
    {
        public AccommodationViewModel AccommodationViewModel { get; } = new AccommodationViewModel();
        public ImageViewModel ImageViewModel { get; } = new ImageViewModel();
        public UserViewModel UserViewModel { get; } = new UserViewModel();
        public WorldViewModel WorldViewModel { get; } = new WorldViewModel();
        public BookingViewModel BookingViewModel { get; } = new BookingViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();
        private bool _skipSelectionChanged;
        public SearchPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await AuthenticationViewModel.IsAnyoneLoggedInAsync();

                await WorldViewModel.GetAllCountriesInTheWorldAsync();

            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }

            await FindUserTypeAsync();
        }

        private async Task FindUserTypeAsync()
        {
            try
            {
                await AuthenticationViewModel.IsEmployeeOrAdminAsync();

                await FillComboboxWithTheIdsOfAllTheCustomersAsync();
            }
            catch (Exception)
            {
            }
        }

        private async Task FillComboboxWithTheIdsOfAllTheCustomersAsync()
        {
            ComboBoxUsers.Visibility = Visibility.Visible;

            var ids = new List<int>();

            await UserViewModel.GetAllCustomersAsync();

            foreach (var user in UserViewModel.AllUsers)
                ids.Add(user.Id);

            ComboBoxUsers.ItemsSource = ids;
        }

        private void ComboBoxUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BookingViewModel.Booking.BookedBy.Id = int.Parse(ComboBoxUsers.SelectedValue.ToString());
        }

        private async void ComboBoxCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxCities.SelectedIndex = -1;

            WorldViewModel.Country = ComboBoxCountries.SelectedValue.ToString();

            await WorldViewModel.GetAllCitiesOfACountryAsync();
        }

        private void ComboBoxCities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxCities.SelectedIndex != -1)
                WorldViewModel.City = ComboBoxCities.SelectedValue.ToString();
        }

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_skipSelectionChanged)
            {
                _skipSelectionChanged = false;

                return;
            }


            var index = listView.SelectedIndex;


            ImageViewModel.Image.Accommodation = AccommodationViewModel.AllAccommodations[index];

            await ImageViewModel.GetAllPicturesAsync();

            contentDialog.Visibility = Visibility.Visible;

            var result = await contentDialog.ShowAsync();

            _skipSelectionChanged = true;
            listView.SelectedItem = null;

            if (result == ContentDialogResult.Primary)
                await BookAsync(index);
        }

        private async Task BookAsync(int index)
        {
            BookingViewModel.StartDate = AccommodationViewModel.FromDate;

            await IfNotAdminOrEmployeeGetIdOfLoggedInCustomerAsync();

            BookingViewModel.Nights = int.Parse(nights.Text);
            BookingViewModel.Booking.Accommodation.Id = AccommodationViewModel.AllAccommodations[index].Id;

            var res = await DialogeMessageAsync($"Are you sure you want to submit the booking?", "Book");

            if ((int)res.Id == 1)
                return;

            await BookTheAccommodationAsync();
        }

        private async Task IfNotAdminOrEmployeeGetIdOfLoggedInCustomerAsync()
        {
            try
            {
                await AuthenticationViewModel.IsEmployeeOrAdminAsync();
            }
            catch (Exception)
            {
                await AuthenticationViewModel.IdOfLoggedInUserAsync();
                BookingViewModel.Booking.BookedBy.Id = AuthenticationViewModel.IdOfLoggedInUser;
            }
        }

        private async Task BookTheAccommodationAsync()
        {
            try
            {
                await BookingViewModel.BookAccommodationAsync();
                await new MessageDialog("Booking successful!").ShowAsync();
                Frame.Navigate(typeof(AllBookingsPage));
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void Button_Click_Search(object sender, RoutedEventArgs e)
        {
            AccommodationViewModel.Accommodation.Address.Country = WorldViewModel.Country;

            AccommodationViewModel.Accommodation.Address.City = WorldViewModel.City;

            await FillListViewAsync();
        }

        private async Task FillListViewAsync()
        {
            try
            {
                await AccommodationViewModel.GetAvailableAsync();

                RatingAsc.Visibility = Visibility.Visible;
                PriceAsc.Visibility = Visibility.Visible;
                SizeAsc.Visibility = Visibility.Visible;
                DistanceAsc.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private void MyDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            AccommodationViewModel.FromDate = DatePickerParser(MyDatePicker);
        }
        private async void Button_Click_SortByRatingAsc(object sender, RoutedEventArgs e)
        {
            await SortAsync("Rating", "Ascending", RatingAsc, RatingDesc);
        }
        private async void Button_Click_SortByRatingDesc(object sender, RoutedEventArgs e)
        {
            await SortAsync("Rating", "Descending", RatingDesc, RatingAsc);
        }
        private async void Button_Click_SortByPriceAsc(object sender, RoutedEventArgs e)
        {
            await SortAsync("Price", "Ascending", PriceAsc, PriceDesc);
        }
        private async void Button_Click_SortByPriceDesc(object sender, RoutedEventArgs e)
        {
            await SortAsync("Price", "Descending", PriceDesc, PriceAsc);
        }
        private async void Button_Click_SortBySizeAsc(object sender, RoutedEventArgs e)
        {
            await SortAsync("Size", "Ascending", SizeAsc, SizeDesc);
        }
        private async void Button_Click_SortBySizeDesc(object sender, RoutedEventArgs e)
        {
            await SortAsync("Size", "Descending", SizeDesc, SizeAsc);
        }
        private async void Button_Click_SortByDistanceAsc(object sender, RoutedEventArgs e)
        {
            await SortAsync("Distance", "Ascending", DistanceAsc, DistanceDesc);
        }
        private async void Button_Click_SortByDistanceDesc(object sender, RoutedEventArgs e)
        {
            await SortAsync("Distance", "Descending", DistanceDesc, DistanceAsc);
        }

        private async Task SortAsync(string sortBy, string ascOrDesc, Button hide, Button show)
        {
            AccommodationViewModel.SortBy = sortBy;
            AccommodationViewModel.AscOrDesc = ascOrDesc;

            await AccommodationViewModel.SortAvailableListAsync();

            listView.ItemsSource = AccommodationViewModel.AllAccommodations;

            hide.Visibility = Visibility.Collapsed;
            show.Visibility = Visibility.Visible;
        }
    }
}
