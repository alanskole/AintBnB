using AintBnB.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AintBnB.Views
{
    public sealed partial class LogoutPage : Page
    {
        public AuthenticationViewModel ViewModel { get; } = new AuthenticationViewModel();

        public LogoutPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await ViewModel.LogoutFromApp();
                await new MessageDialog("Logout ok!").ShowAsync();
                this.Frame.Navigate(typeof(LoginPage));
            }
            catch (Exception)
            {
                await new MessageDialog("Logout not ok!").ShowAsync();
                this.Frame.GoBack();
            }
        }
    }
}
