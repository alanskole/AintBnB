using AintBnB.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AintBnB.CommonMethods.CommonViewMethods;

namespace AintBnB.Views
{
    public sealed partial class CreateAccommodationPage : Page
    {
        public AccommodationViewModel AccommodationViewModel { get; } = new AccommodationViewModel();
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
                ComboBoxCountries.ItemsSource = await EuropeViewModel.GetAllCountriesInEurope();

                AccommodationViewModel.UserId = await AuthenticationViewModel.IdOfLoggedInUser();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void ComboBoxCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AccommodationViewModel.Accommodation.Address.Country = ComboBoxCountries.SelectedValue.ToString();

            EuropeViewModel.Country = ComboBoxCountries.SelectedValue.ToString();

            ComboBoxCities.ItemsSource = await EuropeViewModel.GetAllCitiesOfACountry();
        }

        private void ComboBoxCities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AccommodationViewModel.Accommodation.Address.City = ComboBoxCities.SelectedValue.ToString();
        }

        private async void Button_Click_Create_Accommadtion(object sender, RoutedEventArgs e)
        {
            try
            {
                await AccommodationViewModel.CreateAccommodation();
                await new MessageDialog("Creation ok!").ShowAsync();
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
