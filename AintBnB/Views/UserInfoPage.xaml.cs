using AintBnB.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AintBnB.Views
{
    public sealed partial class UserInfoPage : Page
    {
        public UserViewModel ViewModel { get; } = new UserViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();

        public UserInfoPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.UserId = await AuthenticationViewModel.IdOfLoggedInUser();

                await ViewModel.GetAUser();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }

        }

        private async void Button_Click_UpdateUser(object sender, RoutedEventArgs e)
        {
            try
            {
                await ViewModel.UpdateAUser();
                await new MessageDialog("Update ok!").ShowAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private void Button_Click_ChangePass(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PasswordChangePage));
        }

        private async void Button_Click_DeleteUser(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("This will make your account vanish instantly! Are you sure?");
            dialog.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
            dialog.Commands.Add(new UICommand { Label = "Cancel", Id = 1 });
            var res = await dialog.ShowAsync();

            if ((int)res.Id == 0)
            { 
                await ViewModel.DeleteAUser();
                await AuthenticationViewModel.LogoutFromApp();
                this.Frame.Navigate(typeof(MainPage));
            }
        }
    }
}
