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
        public EuropeViewModel EuropeViewModel { get; } = new EuropeViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();

        public CreateAccommodationPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await FindUserType();

                ComboBoxCountries.ItemsSource = await EuropeViewModel.GetAllCountriesInEurope();

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

                ComboBoxUsers.Visibility = Visibility.Visible;

                List<int> ids = new List<int>();

                foreach (var user in await UserViewModel.GetAllCustomers())
                    ids.Add(user.Id);

                ComboBoxUsers.ItemsSource = ids;
            }
            catch (Exception)
            {
                AccommodationViewModel.UserId = await AuthenticationViewModel.IdOfLoggedInUser();
            }
        }

        private void ComboBoxUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                AccommodationViewModel.UserId = int.Parse(ComboBoxUsers.SelectedValue.ToString());
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async void ComboBoxCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxCities.SelectedIndex = -1;

            EuropeViewModel.Country = ComboBoxCountries.SelectedValue.ToString();

            ComboBoxCities.ItemsSource = await EuropeViewModel.GetAllCitiesOfACountry();

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
                Frame.Navigate(typeof(CreateAccommodationPage));
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
