using AintBnB.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AintBnB.App.Views
{
    public sealed partial class CreateAccommodationPage : Page
    {
        public ImageViewModel ImageViewModel { get; } = new ImageViewModel();
        public AccommodationViewModel AccommodationViewModel { get; } = new AccommodationViewModel();
        public UserViewModel UserViewModel { get; } = new UserViewModel();
        public WorldViewModel WorldViewModel { get; } = new WorldViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();

        public CreateAccommodationPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await FindUserTypeAsync();

            await WorldViewModel.GetAllCountriesInTheWorldAsync();
        }

        private async Task FindUserTypeAsync()
        {
            try
            {
                await AuthenticationViewModel.IsAdminAsync();

                await FillComboboxWithIdsOfAllCustomersAsync();
            }
            catch (Exception)
            {
                AccommodationViewModel.UserId = AuthenticationViewModel.IdOfLoggedInUser;
            }
        }

        private async Task FillComboboxWithIdsOfAllCustomersAsync()
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
            AccommodationViewModel.UserId = int.Parse(ComboBoxUsers.SelectedValue.ToString());
        }

        private async void ComboBoxCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxCities.SelectedIndex = -1;

            WorldViewModel.Country = ComboBoxCountries.SelectedValue.ToString();

            await WorldViewModel.GetAllCitiesOfACountryAsync();

            AccommodationViewModel.Accommodation.Address.Country = ComboBoxCountries.SelectedValue.ToString();
        }

        private void ComboBoxCities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxCities.SelectedIndex != -1)
                AccommodationViewModel.Accommodation.Address.City = ComboBoxCities.SelectedValue.ToString();
        }

        private async void Button_Click_Create_Accommadtion(object sender, RoutedEventArgs e)
        {
            try
            {
                await AccommodationViewModel.CreateAccommodationAsync();
                await new MessageDialog("Creation ok!").ShowAsync();

                Frame.Navigate(typeof(AllAccommodationsPage));
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }
    }
}
