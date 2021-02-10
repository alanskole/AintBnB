using AintBnB.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AintBnB.Views
{
    public sealed partial class PasswordChangePage : Page
    {
        public PasswordChangerViewModel PasswordChangerViewModel { get; } = new PasswordChangerViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();


        public PasswordChangePage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PasswordChangerViewModel.UserId = await AuthenticationViewModel.IdOfLoggedInUser();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void Button_Click_ChangePass(object sender, RoutedEventArgs e)
        {
            try
            {
                await PasswordChangerViewModel.ChangePassword();
                await new MessageDialog("Password changed!").ShowAsync();
                Frame.Navigate(typeof(UserInfoPage));
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }
    }
}
