using AintBnB.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace AintBnB.Views
{
    public sealed partial class AllAccommodationsOfAUserPage : Page
    {
        public AccommodationViewModel AccommodationViewModel { get; } = new AccommodationViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();

        public AllAccommodationsOfAUserPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                AccommodationViewModel.UserId = await AuthenticationViewModel.IdOfLoggedInUser();

                listView.ItemsSource = await AccommodationViewModel.GetAllAccommodationsOfAUser();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AccommodationInfoPage infoPage = new AccommodationInfoPage();
            Content = infoPage;
            infoPage.ComboBoxAccommodations.SelectedIndex = listView.SelectedIndex;
        }
    }
}
