using AintBnB.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AintBnB.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public AuthenticationViewModel ViewModel { get; } = new AuthenticationViewModel();

        public LoginPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try {
                await ViewModel.Login();
                this.Frame.Navigate(typeof(UserInfoPage));
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await ViewModel.IdOfLoggedInUser();
            }
            catch (Exception)
            {
                return;
            }
            await new MessageDialog("Already logged in").ShowAsync();
            this.Frame.Navigate(typeof(UserInfoPage));
        }
    }
}
