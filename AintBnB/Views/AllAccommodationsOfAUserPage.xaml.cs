using AintBnB.Core.Models;
using AintBnB.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace AintBnB.Views
{
    public sealed partial class AllAccommodationsOfAUserPage : Page
    {
        public AccommodationViewModel ViewModel { get; } = new AccommodationViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();

        public AllAccommodationsOfAUserPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.UserId = await AuthenticationViewModel.IdOfLoggedInUser();

                listView.ItemsSource = await ViewModel.GetAllAccommodationsOfAUser();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Accommodation acc = (Accommodation)listView.SelectedItem;

            var container = new StackPanel();

            TextBox sqrmBox = new TextBox()
            {
                Text = acc.SquareMeters.ToString(),
                Header = "Square meters",
            };

            TextBox bedroomsBox = new TextBox()
            {
                Text = acc.AmountOfBedrooms.ToString(),
                Header = "Bedrooms",
                Margin = new Thickness(0, 15, 0, 0)
            };

            TextBox nightlyPriceBox = new TextBox()
            {
                Text = acc.PricePerNight.ToString(),
                Header = "Price per night",
                Margin = new Thickness(0, 15, 0, 0)
            };

            TextBox descriptionBox = new TextBox()
            {
                Text = acc.Description,
                Header = "Description",
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 15, 0, 0)
            };

            container.Children.Add(sqrmBox);
            container.Children.Add(bedroomsBox);
            container.Children.Add(nightlyPriceBox);
            container.Children.Add(descriptionBox);

            var contentDialog = new ContentDialog
            {
                Title = "Update or delete an accommodation",
                Content = container,
                PrimaryButtonText = "Update",
                SecondaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await contentDialog.ShowAsync();
            if (result == ContentDialogResult.Secondary)
            {
                var dialog = new MessageDialog("This will delete the accommodation! Are you sure?");
                dialog.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                dialog.Commands.Add(new UICommand { Label = "Cancel", Id = 1 });
                var res = await dialog.ShowAsync();

                if ((int)res.Id == 0)
                {
                    ViewModel.Accommodation.Id = acc.Id;
                    try
                    {
                        await ViewModel.DeleteAccommodation();
                        await new MessageDialog("Deletion ok!").ShowAsync();
                    }
                    catch (Exception ex)
                    {
                        await new MessageDialog(ex.Message).ShowAsync();
                    }
                }
            }
            else if (result == ContentDialogResult.Primary)
            {
                ViewModel.Accommodation.Id = acc.Id;
                ViewModel.Accommodation.SquareMeters = int.Parse(sqrmBox.Text);
                ViewModel.Accommodation.AmountOfBedrooms = int.Parse(bedroomsBox.Text);
                ViewModel.Accommodation.PricePerNight = int.Parse(nightlyPriceBox.Text);
                ViewModel.Accommodation.Description = descriptionBox.Text;
                await ViewModel.UpdateAccommodation();
            }

            Frame.Navigate(typeof(AllAccommodationsOfAUserPage));
        }
    }
}
