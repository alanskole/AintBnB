using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AintBnB.Core.Models
{
    public class User : INotifyPropertyChanged
    {
        private int _id;
        private string _userName;
        private string _password;
        private string _firstName;
        private string _lastName;
        private UserTypes _userType = UserTypes.Customer;

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                NotifyPropertyChanged("Id");
            }
        }
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                NotifyPropertyChanged("UserName");
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                NotifyPropertyChanged("Password");
            }
        }

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                _firstName = value;
                NotifyPropertyChanged("FirstName");
            }
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                _lastName = value;
                NotifyPropertyChanged("LastName");
            }
        }

        public UserTypes UserType
        {
            get { return _userType; }
            set
            {
                _userType = value;
                NotifyPropertyChanged("UserType");
            }
        }

        public override string ToString()
        {
            return _id + " " + _userName + " " + _firstName + " " + _lastName + " " + _userType;
        }

        public User(string userName, string password, string firstName, string lastName)
        {
            UserName = userName.Trim();
            Password = password.Trim();
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
        }

        public User()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
