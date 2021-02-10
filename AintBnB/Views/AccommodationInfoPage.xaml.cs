using AintBnB.ViewModels;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace AintBnB.Views
{
    public sealed partial class AccommodationInfoPage : Page
    {
        public AccommodationViewModel ViewModel { get; } = new AccommodationViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();

        public AccommodationInfoPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click_Update(object sender, RoutedEventArgs e)
        {
            try
            {
                await ViewModel.UpdateAccommodation();
                await new MessageDialog("Update ok").ShowAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void Button_Click_Expand(object sender, RoutedEventArgs e)
        {
            try
            {
                await ViewModel.ExpandScheduleOfAccommodation();
                await new MessageDialog("Expansion of schedule ok").ShowAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void Button_Click_Delete(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("This will delete the accommodation! Are you sure?");
            dialog.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
            dialog.Commands.Add(new UICommand { Label = "Cancel", Id = 1 });
            var res = await dialog.ShowAsync();

            try
            {
                if ((int)res.Id == 0)
                {
                    await ViewModel.DeleteAccommodation();
                    await new MessageDialog("Deletion ok!").ShowAsync();
                    Frame.Navigate(typeof(AccommodationInfoPage));
                }
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
                ViewModel.UserId = await AuthenticationViewModel.IdOfLoggedInUser();

                List<int> ids = new List<int>();

                foreach (var acc in await ViewModel.GetAllAccommodationsOfAUser())
                    ids.Add(acc.Id);

                ComboBoxAccommodations.ItemsSource = ids;

            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }

            BitmapImage bitmapImage = new BitmapImage();
            List<FileRandomAccessStream> stream = new List<FileRandomAccessStream>();
            List<BitmapImage> bitmapImages = new List<BitmapImage>();
            foreach (var item in ViewModel.Accommodation.Picture)
            {
                stream.Add((FileRandomAccessStream)await item.OpenAsync(FileAccessMode.Read));
            }

            for (int i = 0; i < stream.Count; i++)
            {
                bitmapImages.Add(new BitmapImage());
                bitmapImages[i].SetSource(stream[i]);

            }

            listViewPicture.ItemsSource = bitmapImages;
        }

        private async void ComboBoxAccommodations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ViewModel.Accommodation.Id = int.Parse(ComboBoxAccommodations.SelectedValue.ToString());

                await ViewModel.GetAccommodation();

            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void ListViewPicture_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = listViewPicture.SelectedIndex;


            BitmapImage img = (BitmapImage)listViewPicture.SelectedItem;

            var contentDialog = new ContentDialog
            {
                FullSizeDesired = true,
                Content = new Image()
                {
                    Source = img,
                },
                CloseButtonText = "Cancel",
            };

            ContentDialogResult result = await contentDialog.ShowAsync();
        }
    }
}
