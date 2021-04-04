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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            WhenNavigatedToView(e, ComboBoxAccommodations);
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

            var normalUserLoggedIn = false;
            await FindUserTypeOfLoggedInUser(normalUserLoggedIn);
        }

        private async Task FindUserTypeOfLoggedInUser(bool normalUserLoggedIn)
        {
            var ids = new List<int>();

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
                await FillComboboxWithAccommodationIds(ids, normalUserLoggedIn);
            }
        }

        private async Task FillComboboxWithAccommodationIds(List<int> ids, bool normalUserLoggedIn)
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
            ShowButtons();
            GetPhotos();
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
                await AccommodationViewModel.UpdateAccommodation();
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
                await AccommodationViewModel.ExpandScheduleOfAccommodation();
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
                await AccommodationViewModel.DeleteAccommodation();
                await new MessageDialog("Deletion ok!").ShowAsync();
                Frame.Navigate(typeof(AllAccommodationsPage));

            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void GetPhotos()
        {
            var bmimg = new List<BitmapImage>();

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
                await DeleteThePhoto();
            }

            _skipSelectionChanged = true;
            listViewPicture.SelectedItem = null;
        }

        private async Task DeleteThePhoto()
        {
            var res = await DialogeMessageAsync("This will delete the photo! Are you sure?", "Delete");

            if ((int)res.Id == 1)
                return;

            var index = listViewPicture.SelectedIndex;

            AccommodationViewModel.Accommodation.Picture.Remove(AccommodationViewModel.Accommodation.Picture[index]);

            await AccommodationViewModel.UpdateAccommodation();

            Refresh();
        }

        private async void Button_Click_Upload(object sender, RoutedEventArgs e)
        {
            var sizeBeforeUploading = AccommodationViewModel.Accommodation.Picture.Count;

            await PhotoUpload(AccommodationViewModel.Accommodation.Picture);

            if (sizeBeforeUploading != AccommodationViewModel.Accommodation.Picture.Count)
            {
                await AccommodationViewModel.UpdateAccommodation();
                Refresh();
            }
        }

        private void Refresh()
        {
            var infoPage = new AccommodationInfoPage();
            Content = infoPage;
            infoPage.ComboBoxAccommodations.SelectedIndex = ComboBoxAccommodations.Items.IndexOf(AccommodationViewModel.Accommodation.Id);
        }
    }
}
