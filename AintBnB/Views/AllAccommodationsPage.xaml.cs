using AintBnB.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace AintBnB.Views
{
    public sealed partial class AllAccommodationsPage : Page
    {
        public AccommodationViewModel AccommodationViewMode { get; } = new AccommodationViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();

        public AllAccommodationsPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await CheckIfAnyoneIsLoggedInAsync();

            var normalUserLoggedIn = false;

            try
            {
                await AuthenticationViewModel.IsEmployeeOrAdminAsync();

                listView.ItemsSource = await AccommodationViewMode.GetAllAccommodationsAsync();
            }
            catch (Exception)
            {
                AccommodationViewMode.UserId = await AuthenticationViewModel.IdOfLoggedInUserAsync();
                normalUserLoggedIn = true;
            }
            finally
            {
                if (normalUserLoggedIn)
                {
                    try
                    {
                        listView.ItemsSource = await AccommodationViewMode.GetAllAccommodationsOfAUserAsync();
                    }
                    catch (Exception ex)
                    {
                        await new MessageDialog(ex.Message).ShowAsync();
                    }
                }
            }
        }

        private async Task CheckIfAnyoneIsLoggedInAsync()
        {
            try
            {
                await AuthenticationViewModel.IsAnyoneLoggedInAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Frame.Navigate(typeof(AccommodationInfoPage), listView.SelectedIndex);
        }
    }
}
