using System.Text.RegularExpressions;


namespace AintBnB.BusinessLogic.Helpers
{
    public static class RegexValidator
    {
        public static Regex onlyLettersOneSpaceOrDash = new Regex(@"^([A-Za-z\u00C0-\u00D6\u00D8-\u00f6\u00f8-\u00ff]+([\s-]?[A-Za-z\u00C0-\u00D6\u00D8-\u00f6\u00f8-\u00ff]+){1,})$");
        public static Regex onlyLettersNumbersOneSpaceOrDash = new Regex(@"^([A-Za-z\u00C0-\u00D6\u00D8-\u00f6\u00f8-\u00ff0-9]([\s-]?[A-Za-z\u00C0-\u00D6\u00D8-\u00f6\u00f8-\u00ff0-9]+)*){2,}$");
        public static Regex onlyNumbersFollowedByAnOptionalLetter = new Regex(@"^[1-9]+[0-9]*[A-Za-z]?$");
        public static Regex zipCodeFormatsOfTheWorld = new Regex(@"^[A-Za-z\u00C0-\u00D6\u00D8-\u00f6\u00f8-\u00ff0-9][A-Za-z\u00C0-\u00D6\u00D8-\u00f6\u00f8-\u00ff0-9\- ]{1,11}$");
    }
}
