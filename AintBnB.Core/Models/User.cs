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
        private UserTypes _userType;

        public int Id
        {
            get { return _id; }
            set
            {
                if (value == 0)
                    throw new ArgumentException("Id name cannot be zero");
                _id = value;
                NotifyPropertyChanged("Id");
            }
        }
        public string UserName
        {
            get { return _userName; }
            set
            {
                if (value == null)
                    throw new ArgumentException("Username cannot be null");
                _userName = value;
                NotifyPropertyChanged("UserName");
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (value == null)
                    throw new ArgumentException("Password cannot be null");
                _password = value;
                NotifyPropertyChanged("Password");
            }
        }

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (value == null)
                    throw new ArgumentException("First name cannot be null");
                _firstName = value;
                NotifyPropertyChanged("FirstName");
            }
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (value == null)
                    throw new ArgumentException("Last name cannot be null");
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

        public User(string userName, string password, string firstName, string lastName)
        {
            UserName = userName;
            Password = password;
            FirstName = firstName;
            LastName = lastName;
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
