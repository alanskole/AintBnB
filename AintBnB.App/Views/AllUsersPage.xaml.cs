using AintBnB.App.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AintBnB.App.Views
{
    public sealed partial class AllUsersPage : Page
    {
        public UserViewModel UserViewModel { get; } = new UserViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();
        public PasswordChangerViewModel PasswordChangerViewModel { get; } = new PasswordChangerViewModel();

        public AllUsersPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await UserViewModel.GetAllUsersAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Frame.Navigate(typeof(UserInfoPage), listView.SelectedIndex);
        }
    }
}
