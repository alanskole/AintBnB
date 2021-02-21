using AintBnB.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace AintBnB.Views
{
    public sealed partial class CreateUserPage : Page
    {
        public UserViewModel UserViewModel { get; } = new UserViewModel();
        public CreateUserPage()
        {
            this.InitializeComponent();
        }

        private void Button_Click_Login(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }

        private async void Button_Click_CreateUser(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            bool emReqAcc = false;

            emReqAcc = EmployeeReuestChecker(emReqAcc);

            try
            {
                await UserViewModel.CreateTheUser();

                if (emReqAcc)
                    await new MessageDialog("Created, the account must be approved by admin before it can be used!").ShowAsync();
                else
                    await new MessageDialog("Created, login with the user!").ShowAsync();

                Frame.Navigate(typeof(LoginPage));
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private bool EmployeeReuestChecker(bool emReqAcc)
        {
            return NewMethod(emReqAcc);
        }

        private bool NewMethod(bool emReqAcc)
        {
            if (EmpReq.IsChecked == true)
            {
                UserViewModel.RequestToBecomeEmployee();
                emReqAcc = true;
            }
            return emReqAcc;
        }
    }
}
