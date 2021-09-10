using AintBnB.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static AintBnB.App.CommonMethodsAndProperties.CommonViewMethods;

namespace AintBnB.App.Views
{
    public sealed partial class AccommodationInfoPage : Page
    {
        public AccommodationViewModel AccommodationViewModel { get; } = new AccommodationViewModel();
        public ImageViewModel ImageViewModel { get; } = new ImageViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();
        private bool _skipSelectionChanged;
        private int _selectedIndex = -1;

        public AccommodationInfoPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _selectedIndex = WhenNavigatedToView(e, ComboBoxAccommodations);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var ids = new List<int>();

            var normalUserLoggedIn = false;

            try
            {
                await AuthenticationViewModel.IsAdminAsync();

                await AccommodationViewModel.GetAllAccommodationsAsync();

                foreach (var acc in AccommodationViewModel.AllAccommodations)
                    ids.Add(acc.Id);
            }
            catch (Exception)
            {
                normalUserLoggedIn = true;
            }
            finally
            {
                await FillComboboxWithAccommodationIds(ids, normalUserLoggedIn);
            }
        }

        private async Task FillComboboxWithAccommodationIds(List<int> ids, bool normalUserLoggedIn)
        {
            if (normalUserLoggedIn)
            {
                AccommodationViewModel.UserId = AuthenticationViewModel.IdOfLoggedInUser;

                try
                {
                    await AccommodationViewModel.GetAllAccommodationsOfAUserAsync();
                    foreach (var acc in AccommodationViewModel.AllAccommodations)
                        ids.Add(acc.Id);
                }
                catch (Exception ex)
                {
                    await new MessageDialog(ex.Message).ShowAsync();
                }
            }

            ComboBoxAccommodations.ItemsSource = ids;
        }

        private async void ComboBoxAccommodations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                AccommodationViewModel.Accommodation.Id = int.Parse(ComboBoxAccommodations.SelectedValue.ToString());

                await AccommodationViewModel.GetAccommodationAsync();

                ImageViewModel.Image.Accommodation = AccommodationViewModel.Accommodation;

                await ImageViewModel.GetAllPicturesAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
            ShowButtons();

            await ImageViewModel.GetAllPicturesAsync();
        }

        private void ShowButtons()
        {
            street.Visibility = Visibility.Visible;
            number.Visibility = Visibility.Visible;
            zip.Visibility = Visibility.Visible;
            area.Visibility = Visibility.Visible;
            city.Visibility = Visibility.Visible;
            country.Visibility = Visibility.Visible;
            sqm.Visibility = Visibility.Visible;
            bedrooms.Visibility = Visibility.Visible;
            kmFromCenter.Visibility = Visibility.Visible;
            pricePerNight.Visibility = Visibility.Visible;
            cancellationDeadline.Visibility = Visibility.Visible;
            updateButton.Visibility = Visibility.Visible;
            expand.Visibility = Visibility.Visible;
            expandButton.Visibility = Visibility.Visible;
            avgRating.Visibility = Visibility.Visible;
            amountOfRatings.Visibility = Visibility.Visible;
            description.Visibility = Visibility.Visible;
            deleteButton.Visibility = Visibility.Visible;
            uploadButton.Visibility = Visibility.Visible;
        }

        private async void Button_Click_Update(object sender, RoutedEventArgs e)
        {
            try
            {
                await AccommodationViewModel.UpdateAccommodationAsync();
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
                await AccommodationViewModel.ExpandScheduleOfAccommodationAsync();
                await new MessageDialog("Expansion of schedule ok").ShowAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void Button_Click_Delete(object sender, RoutedEventArgs e)
        {
            var res = await DialogeMessageAsync("This will delete the accommodation! Are you sure?", "Delete");

            if ((int)res.Id == 1)
                return;

            await DeleteAcc();
        }

        private async Task DeleteAcc()
        {
            try
            {
                await AccommodationViewModel.DeleteAccommodationAsync();

                await new MessageDialog("Deletion ok!").ShowAsync();

                Frame.Navigate(typeof(AllAccommodationsPage));
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void ListViewPicture_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_skipSelectionChanged)
            {
                _skipSelectionChanged = false;

                return;
            }

            var img = (BitmapImage)listViewPicture.SelectedItem;

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

            var result = await contentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await DeleteThePhotoAsync();
            }

            _skipSelectionChanged = true;
            listViewPicture.SelectedItem = null;
        }

        private async Task DeleteThePhotoAsync()
        {
            var res = await DialogeMessageAsync("This will delete the photo! Are you sure?", "Delete");

            if ((int)res.Id == 1)
                return;

            var index = listViewPicture.SelectedIndex;

            ImageViewModel.ImageId = ImageViewModel.AllImages[index].Id;

            await ImageViewModel.DeleteAPictureAsync();

            Refresh();
        }

        private async void Button_Click_Upload(object sender, RoutedEventArgs e)
        {
            if (await ImageViewModel.PhotoUploadAsync())
                Refresh();
        }

        private void Refresh()
        {
            Frame.Navigate(typeof(AccommodationInfoPage), _selectedIndex);
        }
    }
}
