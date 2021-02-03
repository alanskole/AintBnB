using System.Globalization;

namespace AintBnB.BusinessLogic.Services
{
    static public class CountryNameFormatter
    {
        public static string FormatName(string str)
        {
            string newString = char.ToUpper(str[0]) + str.Substring(1).ToLower();

            if (newString.Contains(" "))
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                return textInfo.ToTitleCase(newString);
            }
            return newString;
        }
    }
}
