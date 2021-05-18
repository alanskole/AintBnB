using AintBnB.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using static AintBnB.App.CommonMethodsAndProperties.CommonViewMethods;

namespace AintBnB.App.Views
{
    public sealed partial class UserInfoPage : Page
    {
        public UserViewModel UserViewModel { get; } = new UserViewModel();

        public PasswordChangerViewModel PasswordChangerViewModel { get; } = new PasswordChangerViewModel();

        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();
        public UserInfoPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            WhenNavigatedToView(e, ComboBoxUsers);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var userid = await AuthenticationViewModel.IdOfLoggedInUserAsync();
                await FindUserTypeAsync(userid);
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }

        }

        private async Task FindUserTypeAsync(int userid)
        {
            try
            {
                await AuthenticationViewModel.IsEmployeeOrAdminAsync();

                await FillComboBoxWithUserIdsAsync();

                if (ComboBoxUsers.SelectedIndex == -1)
                    ComboBoxUsers.SelectedValue = userid;
            }
            catch (Exception)
            {
                await GetCustomerByIdAsync(userid);
            }
        }

        private async Task FillComboBoxWithUserIdsAsync()
        {
            var ids = new List<int>();

            foreach (var user in await UserViewModel.GetAllUsersAsync())
                ids.Add(user.Id);

            ComboBoxUsers.ItemsSource = ids;
        }

        private async Task GetCustomerByIdAsync(int userid)
        {
            ComboBoxUsers.Visibility = Visibility.Collapsed;

            UserViewModel.User.Id = userid;

            await UserViewModel.GetAUserAsync();

            userIdTextBox.Visibility = Visibility.Visible;
        }

        private async void ComboBoxUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                UserViewModel.User.Id = int.Parse(ComboBoxUsers.SelectedValue.ToString());

                await UserViewModel.GetAUserAsync();

                if (await AuthenticationViewModel.IdOfLoggedInUserAsync() != UserViewModel.User.Id)
                    ChangePasswordButton.Visibility = Visibility.Collapsed;
                else
                    ChangePasswordButton.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }

            await VisibilityDeleteButtonAsync();

            await VisibilityOfMakeEmployeeButtonAsync();
        }

        private async Task VisibilityDeleteButtonAsync()
        {

            try
            {
                await AuthenticationViewModel.IsAdminAsync();

                ShowDeleteButtonIfAdminIsLoggedIn();

            }
            catch (Exception)
            {
                HideDeleteButtonIfEmployeeIsLoggedIn();
            }
        }

        private void ShowDeleteButtonIfAdminIsLoggedIn()
        {

            if (userType.Text != "Admin")
                DeleteButton.Visibility = Visibility.Visible;
            else
                DeleteButton.Visibility = Visibility.Collapsed;
        }

        private void HideDeleteButtonIfEmployeeIsLoggedIn()
        {
            DeleteButton.Visibility = Visibility.Collapsed;
        }

        private async void Button_Click_MakeEmployee(object sender, RoutedEventArgs e)
        {
            try
            {
                await UserViewModel.MakeEmployeeAsync();
                await VisibilityOfMakeEmployeeButtonAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async Task VisibilityOfMakeEmployeeButtonAsync()
        {
            try
            {
                await AuthenticationViewModel.IsAdminAsync();

                if (userType.Text == "RequestToBeEmployee")
                    EmployeeButton.Visibility = Visibility.Visible;
                else
                    EmployeeButton.Visibility = Visibility.Collapsed;
            }
            catch (Exception)
            {

            }
        }

        private async void Button_Click_UpdateUser(object sender, RoutedEventArgs e)
        {
            try
            {
                await UserViewModel.UpdateAUserAsync();
                await new MessageDialog("Update ok!").ShowAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void Button_Click_ChangePass(object sender, RoutedEventArgs e)
        {
            contentDialog.Visibility = Visibility.Visible;

            var result = await contentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {

                PasswordChangerViewModel.UserId = UserViewModel.User.Id;
                try
                {
                    await PasswordChangerViewModel.ChangePasswordAsync();
                    await new MessageDialog("Password changed!").ShowAsync();
                }
                catch (Exception ex)
                {
                    await new MessageDialog(ex.Message).ShowAsync();
                }
                finally
                {
                    old.Password = "";
                    new1.Password = "";
                    new2.Password = "";
                }
            }

        }

        private async void Button_Click_DeleteUser(object sender, RoutedEventArgs e)
        {
            var res = await DialogeMessageAsync("This will make the account vanish instantly! Are you sure?", "Ok");

            if ((int)res.Id == 1)
                return;


            var wasDeleted = false;

            try
            {
                await UserViewModel.DeleteAUserAsync();

                await new MessageDialog("Deletion ok!").ShowAsync();

                wasDeleted = true;
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }

            if (wasDeleted)
                await RedirectToCorrectViewBasedOnUserTypeAsync();
        }

        private async Task RedirectToCorrectViewBasedOnUserTypeAsync()
        {
            try
            {
                await AuthenticationViewModel.IsAdminAsync();
                Frame.Navigate(typeof(AllUsersPage));
            }
            catch (Exception)
            {
                await AuthenticationViewModel.LogoutFromAppAsync();
                Frame.Navigate(typeof(MainPage));
            }
        }
    }
}
