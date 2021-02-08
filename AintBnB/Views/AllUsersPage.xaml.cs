using AintBnB.Core.Models;
using AintBnB.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AintBnB.Views
{
    public sealed partial class AllUsersPage : Page
    {
        public UserViewModel ViewModel { get; } = new UserViewModel();
        public PasswordChangerViewModel PasswordChangerViewModel { get; } = new PasswordChangerViewModel();

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

            PasswordBox oldBox = new PasswordBox()
            {
                Header = "Only fill if you want to change the password. otherwise leave the three next fields empty! \n\nOriginal password",
                Margin = new Thickness(0, 15, 0, 0)
            };

            PasswordBox new1Box = new PasswordBox()
            {
                Header = "New password",
                Margin = new Thickness(0, 15, 0, 0)
            };

            PasswordBox new2Box = new PasswordBox()
            {
                Header = "Confirm new password",
                Margin = new Thickness(0, 15, 0, 0)
            };

            container.Children.Add(firstNameBox);
            container.Children.Add(lastNameBox);
            container.Children.Add(oldBox);
            container.Children.Add(new1Box);
            container.Children.Add(new2Box);

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

                if (oldBox.Password.Trim().Length > 0)
                {
                    PasswordChangerViewModel.Old = oldBox.Password;
                    PasswordChangerViewModel.New1 = new1Box.Password;
                    PasswordChangerViewModel.New2 = new2Box.Password;
                    PasswordChangerViewModel.UserId = user.Id;

                    try
                    {
                        await PasswordChangerViewModel.ChangePassword();
                        await new MessageDialog("Password changed!").ShowAsync();
                    }
                    catch (Exception ex)
                    {
                        await new MessageDialog(ex.Message).ShowAsync();
                    }
                }
            }

            Frame.Navigate(typeof(AllUsersPage));
        }
    }
}
