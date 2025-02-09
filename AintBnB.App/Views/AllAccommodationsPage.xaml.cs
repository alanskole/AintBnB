﻿using AintBnB.App.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace AintBnB.App.Views
{
    public sealed partial class AllAccommodationsPage : Page
    {
        public AccommodationViewModel AccommodationViewMode { get; } = new AccommodationViewModel();
        public AuthenticationViewModel AuthenticationViewModel { get; } = new AuthenticationViewModel();

        public AllAccommodationsPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var normalUserLoggedIn = false;

            try
            {
                await AuthenticationViewModel.IsAdminAsync();

                await AccommodationViewMode.GetAllAccommodationsAsync();
            }
            catch
            {
                AccommodationViewMode.UserId = AuthenticationViewModel.IdOfLoggedInUser;
                normalUserLoggedIn = true;
            }
            finally
            {
                if (normalUserLoggedIn)
                {
                    try
                    {
                        await AccommodationViewMode.GetAllAccommodationsOfAUserAsync();
                    }
                    catch (Exception ex)
                    {
                        await new MessageDialog(ex.Message).ShowAsync();
                    }
                }
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Frame.Navigate(typeof(AccommodationInfoPage), listView.SelectedIndex);
        }
    }
}
