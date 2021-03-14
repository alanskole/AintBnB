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
    public sealed partial class CreateAccommodationPage : Page
    {
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
            await CheckIfAnyoneIsLoggedIn();

            await FindUserType();

            ComboBoxCountries.ItemsSource = await WorldViewModel.GetAllCountriesInTheWorld();
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

        private async Task FindUserType()
        {
            try
            {
                await AuthenticationViewModel.IsEmployeeOrAdmin();

                await FillComboboxWithIdsOfAllCustomers();
            }
            catch (Exception)
            {
                AccommodationViewModel.UserId = await AuthenticationViewModel.IdOfLoggedInUser();
            }
        }

        private async Task FillComboboxWithIdsOfAllCustomers()
        {
            ComboBoxUsers.Visibility = Visibility.Visible;

            List<int> ids = new List<int>();

            foreach (var user in await UserViewModel.GetAllCustomers())
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

            ComboBoxCities.ItemsSource = await WorldViewModel.GetAllCitiesOfACountry();

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
                await AccommodationViewModel.CreateAccommodation();
                await new MessageDialog("Creation ok!").ShowAsync();
                Frame.Navigate(typeof(AllAccommodationsPage));
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void Upload_Click(object sender, RoutedEventArgs e)
        {
            await PhotoUpload(AccommodationViewModel.Accommodation.Picture);
        }
    }
}
