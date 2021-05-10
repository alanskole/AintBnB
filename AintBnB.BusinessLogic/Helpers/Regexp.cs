using System.Text.RegularExpressions;

namespace AintBnB.BusinessLogic.Helpers
{
    internal static class Regexp
    {

        /// <summary>The string must be at least two characters long and can only contain letters, one space or dash between the letters</summary>
        public static Regex onlyLettersOneSpaceOrDash = new Regex(@"^(?=.{2,}$)([\p{Lu}\p{Ll}]+([\s-]?[\p{Lu}\p{Ll}]+))$");

        /// <summary>The string must be at least two characters long and contain only letters, numbers, one space or dash between the letters/numbers</summary>
        public static Regex onlyLettersNumbersOneSpaceOrDash = new Regex(@"^(?=.{2,}$)([\p{Lu}\p{Ll}0-9]([\s-]?[\p{Lu}\p{Ll}0-9]+)*)$");

        /// <summary>The string can only contain numbers followed by an optional letter. The first number must be greated than 0</summary>
        public static Regex onlyNumbersFollowedByAnOptionalLetter = new Regex(@"^[1-9]+[0-9]*[A-Za-z]?$");

        /// <summary>The string must be between 2 and 10 characters long containing only letters and mumbers</summary>
        public static Regex zipCodeFormatsOfTheWorld = new Regex(@"^(?=.{2,10}$)([\p{Lu}\p{Ll}0-9]([\s-]?[\p{Lu}\p{Ll}0-9]+)*)$");
    }
}
