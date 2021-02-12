using AintBnB.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AintBnB.Views
{
    public sealed partial class LoginPage : Page
    {
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();

        public LoginPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click_Login(object sender, RoutedEventArgs e)
        {
            try {
                await AuthenticationViewModel.Login();
                Frame.Navigate(typeof(SearchPage));
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private void Button_Click_CreateUser(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreateUserPage));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await AuthenticationViewModel.IsAnyoneLoggedIn();
            }
            catch (Exception)
            {
                return;
            }
            await new MessageDialog("Already logged in").ShowAsync();
            Frame.Navigate(typeof(SearchPage));
        }
    }
}
