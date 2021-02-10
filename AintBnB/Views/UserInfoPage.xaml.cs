using AintBnB.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AintBnB.Views
{
    public sealed partial class UserInfoPage : Page
    {
        public UserViewModel UserViewModel { get; } = new UserViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();
        public UserInfoPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            int userid = 0;

            try
            {
                userid = await AuthenticationViewModel.IdOfLoggedInUser();

            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }

            try
            {
                await AuthenticationViewModel.IsAdmin();

                List<int> ids = new List<int>();

                foreach (var user in await UserViewModel.GetAllUsers())
                    ids.Add(user.Id);

                ComboBoxUsers.ItemsSource = ids;
            }
            catch (Exception)
            {
                ComboBoxUsers.Visibility = Visibility.Collapsed;

                UserViewModel.User.Id = userid;

                await UserViewModel.GetAUser();

                userIdTextBox.Visibility = Visibility.Visible;
            }

        }

        private async void Button_Click_UpdateUser(object sender, RoutedEventArgs e)
        {
            try
            {
                await UserViewModel.UpdateAUser();
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
                await UserViewModel.DeleteAUser();
                await AuthenticationViewModel.LogoutFromApp();
                Frame.Navigate(typeof(MainPage));
            }
        }

        private async void ComboBoxUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                UserViewModel.User.Id = int.Parse(ComboBoxUsers.SelectedValue.ToString());

                await UserViewModel.GetAUser();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }
    }
}
