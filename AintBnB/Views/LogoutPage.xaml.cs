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
    public sealed partial class LogoutPage : Page
    {
        public AuthenticationViewModel ViewModel { get; } = new AuthenticationViewModel();

        public LogoutPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await ViewModel.LogoutFromApp();
                await new MessageDialog("Logout ok!").ShowAsync();
                this.Frame.Navigate(typeof(LoginPage));
            }
            catch (Exception)
            {
                await new MessageDialog("Logout not ok!").ShowAsync();
                this.Frame.GoBack();
            }
        }
    }
}
