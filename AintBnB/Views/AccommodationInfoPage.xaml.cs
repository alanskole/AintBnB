using AintBnB.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using static AintBnB.CommonMethods.CommonViewMethods;

namespace AintBnB.Views
{
    public sealed partial class AccommodationInfoPage : Page
    {
        public AccommodationViewModel AccommodationViewMode { get; } = new AccommodationViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();
        private bool _skipSelectionChanged;

        public AccommodationInfoPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await AuthenticationViewModel.IsAnyoneLoggedIn();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }

            List<int> ids = new List<int>();

            bool normalUserLoggedIn = false;

            try
            {
                await AuthenticationViewModel.IsAdmin();

                foreach (var acc in await AccommodationViewMode.GetAllAccommodations())
                    ids.Add(acc.Id);
            }
            catch (Exception)
            {
                normalUserLoggedIn = true;
            }
            finally
            {
                if (normalUserLoggedIn)
                {
                    AccommodationViewMode.UserId = await AuthenticationViewModel.IdOfLoggedInUser();

                    try
                    {
                        foreach (var acc in await AccommodationViewMode.GetAllAccommodationsOfAUser())
                            ids.Add(acc.Id);
                    }
                    catch (Exception ex)
                    {
                        await new MessageDialog(ex.Message).ShowAsync();
                    }
                }

                ComboBoxAccommodations.ItemsSource = ids;
            }
        }

        private async void ComboBoxAccommodations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                AccommodationViewMode.Accommodation.Id = int.Parse(ComboBoxAccommodations.SelectedValue.ToString());

                await AccommodationViewMode.GetAccommodation();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }

            await GetPhotos();
        }


        private async void Button_Click_Update(object sender, RoutedEventArgs e)
        {
            try
            {
                await AccommodationViewMode.UpdateAccommodation();
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
                await AccommodationViewMode.ExpandScheduleOfAccommodation();
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
                    await AccommodationViewMode.DeleteAccommodation();
                    await new MessageDialog("Deletion ok!").ShowAsync();
                    Frame.Navigate(typeof(AccommodationInfoPage));
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }


        private async Task GetPhotos()
        {
            List<BitmapImage> bmimg = new List<BitmapImage>();

            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                using (DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0)))
                {
                    foreach (var item in AccommodationViewMode.Accommodation.Picture)
                    {
                        writer.WriteBytes(item);
                        await writer.StoreAsync();

                        BitmapImage image = new BitmapImage();
                        await image.SetSourceAsync(stream);
                        bmimg.Add(image);
                    }
                }
            }

            listViewPicture.ItemsSource = bmimg;
        }

        private async void ListViewPicture_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_skipSelectionChanged)
            {
                _skipSelectionChanged = false;

                return;
            }

            BitmapImage img = (BitmapImage)listViewPicture.SelectedItem;

            var contentDialog = new ContentDialog
            {
                FullSizeDesired = true,
                Content = new Image()
                {
                    Source = img,
                },
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
            };

            ContentDialogResult result = await contentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await DeleteThePhoto();
            }

            _skipSelectionChanged = true;
            listViewPicture.SelectedItem = null;
        }

        private async Task DeleteThePhoto()
        {
            var dialog = new MessageDialog("This will delete the photo! Are you sure?");
            dialog.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
            dialog.Commands.Add(new UICommand { Label = "Cancel", Id = 1 });
            var res = await dialog.ShowAsync();

            if ((int)res.Id == 0)
            {
                int index = listViewPicture.SelectedIndex;

                AccommodationViewMode.Accommodation.Picture.Remove(AccommodationViewMode.Accommodation.Picture[index]);

                await AccommodationViewMode.UpdateAccommodation();

                Refresh();
            }
        }

        private async void Button_Click_Upload(object sender, RoutedEventArgs e)
        {
            int sizeBeforeUploading = AccommodationViewMode.Accommodation.Picture.Count;

            await PhotoUpload(AccommodationViewMode.Accommodation.Picture);

            if (sizeBeforeUploading != AccommodationViewMode.Accommodation.Picture.Count)
            {
                await AccommodationViewMode.UpdateAccommodation();
                Refresh();
            }
        }

        private void Refresh()
        {
            AccommodationInfoPage infoPage = new AccommodationInfoPage();
            Content = infoPage;
            infoPage.ComboBoxAccommodations.SelectedIndex = ComboBoxAccommodations.Items.IndexOf(AccommodationViewMode.Accommodation.Id);
        }
    }
}
