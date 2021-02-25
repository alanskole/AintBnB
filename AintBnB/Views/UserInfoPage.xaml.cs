using AintBnB.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using static AintBnB.CommonMethodsAndProperties.CommonViewMethods;

namespace AintBnB.Views
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
                int userid = await AuthenticationViewModel.IdOfLoggedInUser();
                await FindUserType(userid);
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }

        }

        private async Task FindUserType(int userid)
        {
            try
            {
                await AuthenticationViewModel.IsEmployeeOrAdmin();

                await FillComboBoxWithUserIds();

                if (ComboBoxUsers.SelectedIndex == -1)
                    ComboBoxUsers.SelectedValue = userid;
            }
            catch (Exception)
            {
                await GetCustomerById(userid);
            }
        }

        private async Task FillComboBoxWithUserIds()
        {
            List<int> ids = new List<int>();

            foreach (var user in await UserViewModel.GetAllUsers())
                ids.Add(user.Id);

            ComboBoxUsers.ItemsSource = ids;
        }

        private async Task GetCustomerById(int userid)
        {
            ComboBoxUsers.Visibility = Visibility.Collapsed;

            UserViewModel.User.Id = userid;

            await UserViewModel.GetAUser();

            userIdTextBox.Visibility = Visibility.Visible;
        }

        private async void ComboBoxUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                UserViewModel.User.Id = int.Parse(ComboBoxUsers.SelectedValue.ToString());

                await UserViewModel.GetAUser();

                if (await AuthenticationViewModel.IdOfLoggedInUser() != UserViewModel.User.Id)
                    ChangePasswordButton.Visibility = Visibility.Collapsed;
                else
                    ChangePasswordButton.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }

            await VisibilityDeleteButton();

            await VisibilityOfMakeEmployeeButton();
        }

        private async Task VisibilityDeleteButton()
        {

            try
            {
                await AuthenticationViewModel.IsAdmin();

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
        }

        private void HideDeleteButtonIfEmployeeIsLoggedIn()
        {
            DeleteButton.Visibility = Visibility.Collapsed;
        }

        private async void Button_Click_MakeEmployee(object sender, RoutedEventArgs e)
        {
            try
            {
                await UserViewModel.MakeEmployee();
                await VisibilityOfMakeEmployeeButton();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async Task VisibilityOfMakeEmployeeButton()
        {
            try
            {
                await AuthenticationViewModel.IsAdmin();

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
                await UserViewModel.UpdateAUser();
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

            ContentDialogResult result = await contentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {

                PasswordChangerViewModel.UserId = UserViewModel.User.Id;
                try
                {
                    await PasswordChangerViewModel.ChangePassword();
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


            bool wasDeleted = false;

            try
            {
                await UserViewModel.DeleteAUser();

                await new MessageDialog("Deletion ok!").ShowAsync();

                wasDeleted = true;
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }

            if (wasDeleted)
                await RedirectToCorrectViewBasedOnUserType();
        }

        private async Task RedirectToCorrectViewBasedOnUserType()
        {
            try
            {
                await AuthenticationViewModel.IsAdmin();
                Frame.Navigate(typeof(AllUsersPage));
            }
            catch (Exception)
            {
                await AuthenticationViewModel.LogoutFromApp();
                Frame.Navigate(typeof(MainPage));
            }
        }
    }
}
