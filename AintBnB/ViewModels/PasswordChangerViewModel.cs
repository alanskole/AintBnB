using AintBnB.Helpers;
using AintBnB.CommonMethodsAndProperties;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AintBnB.ViewModels
{
    public class PasswordChangerViewModel : Observable
    {
        private int _userId;
        private string _old;
        private string _new1;
        private string _new2;
        private HttpClientProvider _clientProvider = new HttpClientProvider();
        private string _uri;
        private string _uniquePartOfUri;

        public int UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                NotifyPropertyChanged("UserId");
            }
        }

        public string Old
        {
            get { return _old; }
            set
            {
                _old = value;
                NotifyPropertyChanged("Old");
            }
        }

        public string New1
        {
            get { return _new1; }
            set
            {
                _new1 = value;
                NotifyPropertyChanged("New1");
            }
        }

        public string New2
        {
            get { return _new2; }
            set
            {
                _new2 = value;
                NotifyPropertyChanged("New2");
            }
        }

        public PasswordChangerViewModel()
        {
            _clientProvider.ControllerPartOfUri = "api/user/change/";
            _uri = _clientProvider.LocalHostAddress + _clientProvider.LocalHostPort + _clientProvider.ControllerPartOfUri;
        }

        public async Task ChangePassword()
        {
            _uniquePartOfUri = Old + " " + UserId.ToString() + " " + New1 + " " + New2;

            HttpResponseMessage response = await _clientProvider.client.GetAsync(new Uri(_uri + _uniquePartOfUri));

            if (!response.IsSuccessStatusCode)
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
        }
    }
}
