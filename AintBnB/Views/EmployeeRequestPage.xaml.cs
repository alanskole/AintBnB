using AintBnB.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static AintBnB.CommonMethodsAndProperties.CommonViewMethods;


namespace AintBnB.Views
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
                await AuthenticationViewModel.IsAdmin();

                listView.ItemsSource = await UserViewModel.GetAllEmployeeRequests();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_skipSelectionChanged)
            {
                _skipSelectionChanged = false;

                return;
            }

            UserViewModel.User.Id = UserViewModel.AllEmployeeRequests[listView.SelectedIndex].Id;

            contentDialog.Visibility = Visibility.Visible;

            ContentDialogResult result = await contentDialog.ShowAsync();

            _skipSelectionChanged = true;
            listView.SelectedItem = null;

            if (result == ContentDialogResult.Primary)
            {
                var res = await DialogeMessageAsync("Are you sure want to approve the account?", "Approve");

                if ((int)res.Id == 1)
                    return;

                await Approve();
            }

            if (result == ContentDialogResult.Secondary)
            {
                var res = await DialogeMessageAsync("This will make the account vanish instantly! Are you sure?", "Delete");


                if ((int)res.Id == 1)
                    return;

                await Delete();
            }
        }

        private async Task Approve()
        {
            try
            {
                await UserViewModel.GetAUser();

                await UserViewModel.MakeEmployee();

                await new MessageDialog("Successfully approved employee request!").ShowAsync();

                Frame.Navigate(typeof(AllUsersPage));
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async Task Delete()
        {
            try
            {
                await UserViewModel.DeleteAUser();

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
