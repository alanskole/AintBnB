using System.Net;

namespace AintBnB.Core.Models
{
    public class User
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
                _id = value;
            }
        }
        public string UserName
        {
            get { return WebUtility.HtmlDecode(_userName); }
            set
            {
                _userName = WebUtility.HtmlEncode(value);
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
            }
        }

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                _firstName = value;
            }
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                _lastName = value;
            }
        }

        public UserTypes UserType
        {
            get { return _userType; }
            set
            {
                _userType = value;
            }
        }

        public override string ToString()
        {
            return "User ID: " + Id + ", Username: " + UserName + ", Firstname: " + FirstName + ", Lastname: " + LastName + ", Usertype: " + UserType;
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
    }
}
