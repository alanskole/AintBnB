using AintBnB.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace AintBnB.Views
{
    public sealed partial class CreateUserPage : Page
    {
        public UserViewModel ViewModel { get; } = new UserViewModel();
        public CreateUserPage()
        {
            this.InitializeComponent();
        }

        private void Button_Click_Login(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }

        private async void Button_Click_CreateUser(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                await ViewModel.CreateTheUser();

                await new MessageDialog("Created, login with the user!").ShowAsync();

                this.Frame.Navigate(typeof(LoginPage));
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }
    }
}
