using AintBnB.Core.Models;
using AintBnB.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AintBnB.Views
{
    public sealed partial class AllUsersPage : Page
    {
        public UserViewModel ViewModel { get; } = new UserViewModel();

        public AllUsersPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                listView.ItemsSource = await ViewModel.GetAllUsers();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = listView.SelectedIndex;

            List<User> userList = await ViewModel.GetAllUsers();

            User user = userList[index];

            var container = new StackPanel();

            TextBox firstNameBox = new TextBox()
            {
                Text = user.FirstName,
                Header = "Firstname",
            };

            TextBox lastNameBox = new TextBox()
            {
                Text = user.LastName,
                Header = "Lastname",
                Margin = new Thickness(0, 15, 0, 0)
            };

            container.Children.Add(firstNameBox);
            container.Children.Add(lastNameBox);

            var contentDialog = new ContentDialog
            {
                Title = "Update or delete a user",
                Content = container,
                PrimaryButtonText = "Update",
                SecondaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await contentDialog.ShowAsync();
            if (result == ContentDialogResult.Secondary)
            {
                var dialog = new MessageDialog("This will make the account vanish instantly! Are you sure?");
                dialog.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                dialog.Commands.Add(new UICommand { Label = "Cancel", Id = 1 });
                var res = await dialog.ShowAsync();

                if ((int)res.Id == 0)
                {
                    ViewModel.UserId = user.Id;
                    await ViewModel.DeleteAUser();
                }
            }
            else if (result == ContentDialogResult.Primary)
            {
                ViewModel.UserId = user.Id;
                ViewModel.User.FirstName = firstNameBox.Text;
                ViewModel.User.LastName = lastNameBox.Text;
                ViewModel.User.UserName = user.UserName;
                ViewModel.User.Password = user.Password;
                await ViewModel.UpdateAUser();
            }

            Frame.Navigate(typeof(AllUsersPage));
        }
    }
}
