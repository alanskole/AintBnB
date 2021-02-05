using AintBnB.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AintBnB.Views
{
    public sealed partial class AllUsersPage : Page
    {
        public UserInfoViewModel ViewModel { get; } = new UserInfoViewModel();

        public AllUsersPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            listView.ItemsSource = await ViewModel.GetAllUsers();
        }
    }
}
