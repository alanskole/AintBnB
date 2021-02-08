using AintBnB.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AintBnB.Views
{
    public sealed partial class LoginPage : Page
    {
        public AuthenticationViewModel ViewModel { get; } = new AuthenticationViewModel();

        public LoginPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try {
                await ViewModel.Login();
                this.Frame.Navigate(typeof(UserInfoPage));
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await ViewModel.IdOfLoggedInUser();
            }
            catch (Exception)
            {
                return;
            }
            await new MessageDialog("Already logged in").ShowAsync();
            this.Frame.Navigate(typeof(UserInfoPage));
        }
    }
}
