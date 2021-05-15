using AintBnB.CommonMethodsAndProperties;
using AintBnB.Core.Models;
using AintBnB.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AintBnB.CommonMethodsAndProperties.ApiCalls;

namespace AintBnB.ViewModels
{
    public class ImageViewModel : Observable
    {
        private HttpClientProvider _clientProvider = new HttpClientProvider();
        private string _uri;
        private string _uniquePartOfUri;
        private Image _image = new Image();
        private int _imageId;
        private int _accommodationId;
        private List<Image> _allImages;

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

        public int AccommodationId
        {
            get { return _accommodationId; }
            set
            {
                _accommodationId = value;
                NotifyPropertyChanged("AccommodationId");
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

        public ImageViewModel()
        {
            _clientProvider.ControllerPartOfUri = "api/image/";
            _uri = _clientProvider.LocalHostAddress + _clientProvider.LocalHostPort + _clientProvider.ControllerPartOfUri;
        }
        public async Task CreatePictureAsync()
        {
            await PostAsync(_uri, Image, _clientProvider);
        }

        public async Task<List<Image>> GetAllPicturesAsync()
        {
            _uniquePartOfUri = _accommodationId.ToString();

            _allImages = await GetAllAsync<Image>(_uri + _uniquePartOfUri, _clientProvider);

            return _allImages;
        }

        public async Task DeleteAPictureAsync()
        {
            _uniquePartOfUri = _imageId.ToString();

            await DeleteAsync(_uri + _uniquePartOfUri, _clientProvider);
        }
    }
}
