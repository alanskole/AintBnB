using AintBnB.App.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AintBnB.App.Views
{
    public sealed partial class LogoutPage : Page
    {
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();

        public LogoutPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await AuthenticationViewModel.LogoutFromAppAsync();
                await new MessageDialog("Logout ok!").ShowAsync();
                Frame.Navigate(typeof(MainPage));
            }
            catch (Exception)
            {
                await new MessageDialog("Logout not ok!").ShowAsync();
                Frame.GoBack();
            }
        }
    }
}
