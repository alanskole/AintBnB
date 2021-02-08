using AintBnB.Core.Models;
using AintBnB.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace AintBnB.Views
{
    public sealed partial class AllAccommodationsPage : Page
    {
        public AccommodationViewModel ViewModel { get; } = new AccommodationViewModel();

        public AllAccommodationsPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                listView.ItemsSource = await ViewModel.GetAllAccommodations();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            int index = listView.SelectedIndex;

            List<Accommodation> accList = await ViewModel.GetAllAccommodations();

            Accommodation acc = accList[index];

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
                var dialog = new MessageDialog("This will make the account vanish instantly! Are you sure?");
                dialog.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                dialog.Commands.Add(new UICommand { Label = "Cancel", Id = 1 });
                var res = await dialog.ShowAsync();

                if ((int)res.Id == 0)
                {
                    ViewModel.AccommodationId = acc.Id;
                    await ViewModel.DeleteAccommodation();
                }
            }
            else if (result == ContentDialogResult.Primary)
            {
                ViewModel.AccommodationId = acc.Id;
                ViewModel.Accommodation.SquareMeters = int.Parse(sqrmBox.Text);
                ViewModel.Accommodation.AmountOfBedrooms = int.Parse(bedroomsBox.Text);
                ViewModel.Accommodation.PricePerNight = int.Parse(nightlyPriceBox.Text);
                ViewModel.Accommodation.Description = descriptionBox.Text;
                await ViewModel.UpdateAccommodation();
            }

            Frame.Navigate(typeof(AllAccommodationsPage));
        }
    }
}
