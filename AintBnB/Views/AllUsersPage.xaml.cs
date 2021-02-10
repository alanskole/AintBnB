using AintBnB.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AintBnB.Views
{
    public sealed partial class AllUsersPage : Page
    {
        public UserViewModel UserViewModel { get; } = new UserViewModel();
        public PasswordChangerViewModel PasswordChangerViewModel { get; } = new PasswordChangerViewModel();

        public AllUsersPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                listView.ItemsSource = await UserViewModel.GetAllUsers();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserInfoPage infoPage = new UserInfoPage();
            Content = infoPage;
            infoPage.ComboBoxUsers.SelectedIndex = listView.SelectedIndex;
        }
    }
}
