using AintBnB.ViewModels;
using System;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace AintBnB.Views
{
    public sealed partial class CreateAccommodationPage : Page
    {
        public AccommodationViewModel ViewModel { get; } = new AccommodationViewModel();
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

                ViewModel.UserId = await AuthenticationViewModel.IdOfLoggedInUser();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void ComboBoxCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.Accommodation.Address.Country = ComboBoxCountries.SelectedValue.ToString();

            EuropeViewModel.Country = ComboBoxCountries.SelectedValue.ToString();

            ComboBoxCities.ItemsSource = await EuropeViewModel.GetAllCitiesOfACountry();
        }

        private void ComboBoxCities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.Accommodation.Address.City = ComboBoxCities.SelectedValue.ToString();
        }

        private async void Button_Click_Create_Accommadtion(object sender, RoutedEventArgs e)
        {
            try
            {
                await ViewModel.CreateAccommodation();
                await new MessageDialog("Creation ok!").ShowAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void Upload_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".jppg");
            fileOpenPicker.FileTypeFilter.Add(".png");

            ViewModel.Accommodation.Picture.Add(await fileOpenPicker.PickSingleFileAsync());

            if (ViewModel.Accommodation.Picture == null)
            {
                // The user cancelled the picking operation
                return;
            }
        }
    }
}
