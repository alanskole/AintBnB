using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace AintBnB.CommonMethodsAndProperties
{
    static public class CommonViewMethods
    {
        public static void WhenNavigatedToView(NavigationEventArgs e, ComboBox comboBox)
        {
            if (e.Parameter != null)
            {
                var parameter = int.Parse(e.Parameter.ToString());

                comboBox.SelectedIndex = parameter;
            }
        }

        public static async Task<IUICommand> DialogeMessageAsync(string message, string buttonText)
        {
            var dialog = new MessageDialog(message);
            dialog.Commands.Add(new UICommand { Label = buttonText, Id = 0 });
            dialog.Commands.Add(new UICommand { Label = "Cancel", Id = 1 });
            var res = await dialog.ShowAsync();

            return res;
        }

        public static string DatePickerParser(CalendarDatePicker datePicker)
        {
            var date = datePicker.Date;

            DateTime dt = date.Value.DateTime;

            return dt.ToString("yyyy-MM-dd");
        }

        public static async Task PhotoUpload(List<byte[]> picture)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                using (var inputStream = await file.OpenSequentialReadAsync())
                {
                    var readStream = inputStream.AsStreamForRead();
                    byte[] buffer = new byte[readStream.Length];
                    await readStream.ReadAsync(buffer, 0, buffer.Length);
                    picture.Add(buffer);
                }
            }
        }

        public static async Task ConvertBytesToBitmapImageList(List<byte[]> originalBytes, List<BitmapImage> convertedToBitmapImage)
        {
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                using (DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0)))
                {
                    foreach (var item in originalBytes)
                    {
                        writer.WriteBytes(item);
                        await writer.StoreAsync();

                        BitmapImage image = new BitmapImage();
                        await image.SetSourceAsync(stream);
                        convertedToBitmapImage.Add(image);
                    }
                }
            }
        }
    }
}
