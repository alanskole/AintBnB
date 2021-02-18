using AintBnB.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using static AintBnB.CommonMethodsAndProperties.CommonViewMethods;

namespace AintBnB.Views
{
    public sealed partial class BookingUpdatePage : Page
    {
        public BookingViewModel BookingViewModel { get; } = new BookingViewModel();

        public BookingUpdatePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                var parameter = int.Parse(e.Parameter.ToString());

                BookingViewModel.Booking.Id = parameter;
            }
        }

        private void MyDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            BookingViewModel.StartDate = DatePickerParser(MyDatePicker);
        }

        private async void Button_Click_Update(object sender, RoutedEventArgs e)
        {
            try
            {
                await BookingViewModel.UpdateBooking();
                await new MessageDialog("Update ok!").ShowAsync();
                Frame.Navigate(typeof(BookingInfoPage), BookingViewModel.Booking.Id);
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }
    }
}
