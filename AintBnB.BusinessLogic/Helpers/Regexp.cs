using System.Text.RegularExpressions;

namespace AintBnB.BusinessLogic.Helpers
{
    public static class Regexp
    {
        public static Regex onlyLettersOneSpaceOrDash = new Regex(@"^(?=.{2,}$)([\p{Lu}\p{Ll}]+([\s-]?[\p{Lu}\p{Ll}]+))$");
        public static Regex onlyLettersNumbersOneSpaceOrDash = new Regex(@"^(?=.{2,}$)([\p{Lu}\p{Ll}0-9]([\s-]?[\p{Lu}\p{Ll}0-9]+)*)$");
        public static Regex onlyNumbersFollowedByAnOptionalLetter = new Regex(@"^[1-9]+[0-9]*[A-Za-z]?$");
        public static Regex zipCodeFormatsOfTheWorld = new Regex(@"^(?=.{2,10}$)([\p{Lu}\p{Ll}0-9]([\s-]?[\p{Lu}\p{Ll}0-9]+)*)$");
    }
}
