namespace AintBnB.Core.Models
{
    public class Image
    {
        private int _id;
        private Accommodation _accommodation;
        private byte[] _img;

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
            }
        }

        public Accommodation Accommodation
        {
            get { return _accommodation; }
            set
            {
                _accommodation = value;
            }
        }

        public byte[] Img
        {
            get { return _img; }
            set
            {
                _img = value;
            }
        }

        public Image()
        {

        }

        public Image(Accommodation accommodation, byte[] img)
        {
            _accommodation = accommodation;
            _img = img;
        }
    }
}
