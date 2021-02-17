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
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();
        public UserInfoPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            int userid = await IdOfLoggedInUser();
            await FindUserType(userid);
            await VisibilityOfMakeEmployeeButton();
            IsComboBoxEmpty();
        }

        private async Task<int> IdOfLoggedInUser()
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

            return userid;
        }

        private async Task FindUserType(int userid)
        {
            try
            {
                await AuthenticationViewModel.IsEmployeeOrAdmin();

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

                ShowButtons();
            }
        }

        private async Task VisibilityOfMakeEmployeeButton()
        {
            try
            {
                await AuthenticationViewModel.IsAdmin();

                if (userType.Text == "Customer")
                    EmployeeButton.Visibility = Visibility.Visible;
                else
                    EmployeeButton.Visibility = Visibility.Collapsed;
            }
            catch (Exception)
            {

            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            WhenNavigatedToView(e, ComboBoxUsers);
        }

        private async void Button_Click_MakeEmployee(object sender, RoutedEventArgs e)
        {
            await UserViewModel.MakeEmployee();
        }

        private async void Button_Click_UpdateUser(object sender, RoutedEventArgs e)
        {
            try
            {
                await UserViewModel.UpdateAUser();
                await new MessageDialog("Update ok!").ShowAsync();
                Frame.Navigate(typeof(UserInfoPage));
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private void Button_Click_ChangePass(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PasswordChangePage), UserViewModel.User.Id);
        }

        private async void Button_Click_DeleteUser(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("This will make your account vanish instantly! Are you sure?");
            dialog.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
            dialog.Commands.Add(new UICommand { Label = "Cancel", Id = 1 });
            var res = await dialog.ShowAsync();

            if ((int)res.Id == 0)
            {
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

                try
                {
                    await AuthenticationViewModel.IsAdmin();
                    Frame.Navigate(typeof(AllUsersPage));
                }
                catch (Exception)
                {
                    if (wasDeleted)
                    {
                        await AuthenticationViewModel.LogoutFromApp();
                        Frame.Navigate(typeof(MainPage));
                    }

                }
            }
        }

        private async void ComboBoxUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                UserViewModel.User.Id = int.Parse(ComboBoxUsers.SelectedValue.ToString());

                await UserViewModel.GetAUser();

                IsComboBoxEmpty();

            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private void IsComboBoxEmpty()
        {
            if (ComboBoxUsers.Visibility == Visibility.Visible && ComboBoxUsers.SelectedIndex == -1)
                HideButtonsWhenNoUserChosen();
            else
                ShowButtons();
        }

        private async void ShowButtons()
        {
            userName.Visibility = Visibility.Visible;
            firstName.Visibility = Visibility.Visible;
            lastName.Visibility = Visibility.Visible;
            userType.Visibility = Visibility.Visible;
            UpdateUserButton.Visibility = Visibility.Visible;
            ChangePasswordButton.Visibility = Visibility.Visible;

            VisibilityDeleteButton();

            await VisibilityOfMakeEmployeeButton();
        }

        private void HideButtonsWhenNoUserChosen()
        {
            userName.Visibility = Visibility.Collapsed;
            firstName.Visibility = Visibility.Collapsed;
            lastName.Visibility = Visibility.Collapsed;
            userType.Visibility = Visibility.Collapsed;
            EmployeeButton.Visibility = Visibility.Collapsed;
            UpdateUserButton.Visibility = Visibility.Collapsed;
            ChangePasswordButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
        }

        private async void VisibilityDeleteButton()
        {
            try
            {
                await AuthenticationViewModel.IsAdmin();

                DeleteButton.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {
            }

            if (userType.Text == "Admin")
                DeleteButton.Visibility = Visibility.Collapsed;

            try
            {
                await AuthenticationViewModel.IsEmployee();
                DeleteButton.Visibility = Visibility.Collapsed;
            }
            catch (Exception)
            {
            }
        }
    }
}
