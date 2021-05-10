using AintBnB.CommonMethodsAndProperties;
using AintBnB.Helpers;
using System.Threading.Tasks;
using static AintBnB.CommonMethodsAndProperties.ApiCalls;

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

        public async Task ChangePasswordAsync()
        {
            var elements = new string[] { Old, UserId.ToString(), New1, New2 };

            await PostAsync(_uri, elements, _clientProvider);
        }
    }
}
