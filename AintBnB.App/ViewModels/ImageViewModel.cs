﻿using AintBnB.App.Helpers;
using AintBnB.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using static AintBnB.App.Helpers.ApiCalls;
using static AintBnB.App.Helpers.UwpCookieHelper;

namespace AintBnB.App.ViewModels
{
    public class ImageViewModel : Observable
    {

        private string _uri = "image/";
        private Image _image = new Image();
        private int _imageId;
        private List<Image> _allImages;
        private List<byte[]> _allImagesBytes;
        private List<BitmapImage> _allImagesConverted;


        public Image Image
        {
            get { return _image; }
            set
            {
                _image = value;
                NotifyPropertyChanged("Image");
            }
        }

        public int ImageId
        {
            get { return _imageId; }
            set
            {
                _imageId = value;
                NotifyPropertyChanged("ImageId");
            }
        }

        public List<Image> AllImages
        {
            get { return _allImages; }
            set
            {
                _allImages = value;
                NotifyPropertyChanged("AllImages");
            }
        }

        public List<byte[]> AllImagesBytes
        {
            get { return _allImagesBytes; }
            set
            {
                _allImagesBytes = value;
                NotifyPropertyChanged("AllImagesBytes");
            }
        }

        public List<BitmapImage> AllImagesConverted
        {
            get { return _allImagesConverted; }
            set
            {
                _allImagesConverted = value;
                NotifyPropertyChanged("AllImagesConverted");
            }
        }

        public async Task CreatePictureAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                await AddAuthCookieAsync(_clientProvider.clientHandler);

                await GetCsrfToken(_clientProvider);

                await PostAsync(_uri, Image, _clientProvider);
            }
        }

        public async Task GetAllPicturesAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var uniquePartOfUri = $"all/{_image.Accommodation.Id}";

                await AddAuthCookieAsync(_clientProvider.clientHandler);

                AllImages = await GetAllAsync<Image>($"{_uri}{uniquePartOfUri}", _clientProvider);

                _allImagesBytes = new List<byte[]>();

                foreach (var pic in AllImages)
                {
                    _allImagesBytes.Add(pic.Img);
                }

                await ConvertBytesToBitmapImageListAsync();
            }
        }

        private async Task ConvertBytesToBitmapImageListAsync()
        {
            using (var stream = new InMemoryRandomAccessStream())
            {
                using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
                {
                    _allImagesConverted = new List<BitmapImage>();

                    foreach (var item in _allImagesBytes)
                    {
                        writer.WriteBytes(item);
                        await writer.StoreAsync();

                        var image = new BitmapImage();
                        await image.SetSourceAsync(stream);
                        _allImagesConverted.Add(image);
                    }
                    NotifyPropertyChanged("AllImagesConverted");
                }
            }
        }

        public async Task<bool> PhotoUploadAsync()
        {
            if (_allImagesBytes == null)
                _allImagesBytes = new List<byte[]>();

            var originalSize = _allImagesBytes.Count;

            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                using (var inputStream = await file.OpenSequentialReadAsync())
                {
                    var readStream = inputStream.AsStreamForRead();
                    byte[] buffer = new byte[readStream.Length];
                    await readStream.ReadAsync(buffer, 0, buffer.Length);
                    _allImagesBytes.Add(buffer);
                }
            }

            return await WasAnyPhotosUploadedAsync(originalSize);
        }

        private async Task<bool> WasAnyPhotosUploadedAsync(int originalSize)
        {
            if (originalSize + 1 == _allImagesBytes.Count)
            {
                Image.Img = _allImagesBytes[_allImagesBytes.Count - 1];
                await CreatePictureAsync();

                return true;
            }
            return false;
        }

        public async Task DeleteAPictureAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var uniquePartOfUri = _imageId;

                await AddAuthCookieAsync(_clientProvider.clientHandler);

                await GetCsrfToken(_clientProvider);

                await DeleteAsync($"{_uri}{uniquePartOfUri}", _clientProvider);
            }
        }
    }
}
