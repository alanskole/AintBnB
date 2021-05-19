using AintBnB.App.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AintBnB.App.CommonMethodsAndProperties.CommonViewMethods;


namespace AintBnB.App.Views
{
    public sealed partial class EmployeeRequestPage : Page
    {
        public UserViewModel UserViewModel { get; } = new UserViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();
        private bool _skipSelectionChanged;

        public EmployeeRequestPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await AuthenticationViewModel.IsAdminAsync();

                await UserViewModel.GetAllEmployeeRequestsAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_skipSelectionChanged)
            {
                _skipSelectionChanged = false;

                return;
            }

            UserViewModel.User.Id = UserViewModel.AllUsers[listView.SelectedIndex].Id;

            contentDialog.Visibility = Visibility.Visible;

            var result = await contentDialog.ShowAsync();

            _skipSelectionChanged = true;
            listView.SelectedItem = null;

            if (result == ContentDialogResult.Primary)
            {
                var res = await DialogeMessageAsync("Are you sure want to approve the account?", "Approve");

                if ((int)res.Id == 1)
                    return;

                await ApproveAsync();
            }

            if (result == ContentDialogResult.Secondary)
            {
                var res = await DialogeMessageAsync("This will make the account vanish instantly! Are you sure?", "Delete");


                if ((int)res.Id == 1)
                    return;

                await DeleteAsync();
            }
        }

        private async Task ApproveAsync()
        {
            try
            {
                await UserViewModel.GetAUserAsync();

                await UserViewModel.MakeEmployeeAsync();

                await new MessageDialog("Successfully approved employee request!").ShowAsync();

                Frame.Navigate(typeof(AllUsersPage));
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async Task DeleteAsync()
        {
            try
            {
                await UserViewModel.DeleteAUserAsync();

                await new MessageDialog("Account deleted!").ShowAsync();

                Frame.Navigate(typeof(AllUsersPage));
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }
    }
}
