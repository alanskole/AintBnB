using AintBnB.App.CommonMethodsAndProperties;
using AintBnB.App.Helpers;
using System.Threading.Tasks;
using static AintBnB.App.CommonMethodsAndProperties.ApiCalls;
using static AintBnB.App.Helpers.UwpCookieHelper;

namespace AintBnB.App.ViewModels
{
    public class PasswordChangerViewModel : Observable
    {
        private int _userId;
        private string _old;
        private string _new1;
        private string _new2;
        private string _uri = "user/change/";

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

        public async Task ChangePasswordAsync()
        {
            using (var _clientProvider = new HttpClientProvider())
            {
                var elements = new string[] { Old, UserId.ToString(), New1, New2 };

                await AddAuthCookieAsync(_clientProvider.clientHandler);

                await GetCsrfToken(_clientProvider);

                await PostAsync(_uri, elements, _clientProvider);
            }
        }
    }
}
