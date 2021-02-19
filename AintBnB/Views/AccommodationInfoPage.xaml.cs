using AintBnB.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static AintBnB.CommonMethodsAndProperties.CommonViewMethods;

namespace AintBnB.Views
{
    public sealed partial class AccommodationInfoPage : Page
    {
        public AccommodationViewModel AccommodationViewModel { get; } = new AccommodationViewModel();
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
                await AuthenticationViewModel.IsEmployeeOrAdmin();

                foreach (var acc in await AccommodationViewModel.GetAllAccommodations())
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
                    AccommodationViewModel.UserId = await AuthenticationViewModel.IdOfLoggedInUser();

                    try
                    {
                        foreach (var acc in await AccommodationViewModel.GetAllAccommodationsOfAUser())
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            WhenNavigatedToView(e, ComboBoxAccommodations);
        }

        private async void ComboBoxAccommodations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                AccommodationViewModel.Accommodation.Id = int.Parse(ComboBoxAccommodations.SelectedValue.ToString());

                await AccommodationViewModel.GetAccommodation();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }

            GetPhotos();
        }


        private async void Button_Click_Update(object sender, RoutedEventArgs e)
        {
            try
            {
                await AccommodationViewModel.UpdateAccommodation();
                await new MessageDialog("Update ok").ShowAsync();
                Frame.Navigate(typeof(AccommodationInfoPage));
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
                await AccommodationViewModel.ExpandScheduleOfAccommodation();
                await new MessageDialog("Expansion of schedule ok").ShowAsync();
                Frame.Navigate(typeof(AccommodationInfoPage));
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
                    await AccommodationViewModel.DeleteAccommodation();
                    await new MessageDialog("Deletion ok!").ShowAsync();
                    Frame.Navigate(typeof(AccommodationInfoPage));
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }


        private async void GetPhotos()
        {
            List<BitmapImage> bmimg = new List<BitmapImage>();

            await ConvertBytesToBitmapImageList(AccommodationViewModel.Accommodation.Picture, bmimg);

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
                Content = new Image()
                {
                    Width = 480,
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

                AccommodationViewModel.Accommodation.Picture.Remove(AccommodationViewModel.Accommodation.Picture[index]);

                await AccommodationViewModel.UpdateAccommodation();

                Refresh();
            }
        }

        private async void Button_Click_Upload(object sender, RoutedEventArgs e)
        {
            int sizeBeforeUploading = AccommodationViewModel.Accommodation.Picture.Count;

            await PhotoUpload(AccommodationViewModel.Accommodation.Picture);

            if (sizeBeforeUploading != AccommodationViewModel.Accommodation.Picture.Count)
            {
                await AccommodationViewModel.UpdateAccommodation();
                Refresh();
            }
        }

        private void Refresh()
        {
            AccommodationInfoPage infoPage = new AccommodationInfoPage();
            Content = infoPage;
            infoPage.ComboBoxAccommodations.SelectedIndex = ComboBoxAccommodations.Items.IndexOf(AccommodationViewModel.Accommodation.Id);
        }
    }
}
