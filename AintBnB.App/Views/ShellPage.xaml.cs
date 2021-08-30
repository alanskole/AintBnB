using AintBnB.App.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AintBnB.App.Views
{
    public sealed partial class ShellPage : Page
    {
        public ShellViewModel ViewModel { get; } = new ShellViewModel();

        public ShellPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
            ViewModel.Initialize(shellFrame, navigationView, KeyboardAccelerators);
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            //if (e.SourcePageType == typeof(MainPage) || e.SourcePageType == typeof(LoginPage) || e.SourcePageType == typeof(CreateUserPage))
            //    navigationView.IsPaneVisible = false;
            //else
            navigationView.IsPaneVisible = true;
        }
    }
}
