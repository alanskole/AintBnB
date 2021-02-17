using AintBnB.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AintBnB.Views
{
    public sealed partial class PasswordChangePage : Page
    {
        public PasswordChangerViewModel PasswordChangerViewModel { get; } = new PasswordChangerViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();


        public PasswordChangePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                var parameter = int.Parse(e.Parameter.ToString());

                PasswordChangerViewModel.UserId = parameter;
            }
        }

        private async void Button_Click_ChangePass(object sender, RoutedEventArgs e)
        {
            try
            {
                await PasswordChangerViewModel.ChangePassword();
                await new MessageDialog("Password changed!").ShowAsync();
                Frame.Navigate(typeof(UserInfoPage));
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }
    }
}
