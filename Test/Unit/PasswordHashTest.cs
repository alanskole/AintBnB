using NUnit.Framework;
using static AintBnB.BusinessLogic.Helpers.PasswordHashing;

namespace Test.Unit
{
    [TestFixture]
    public class PasswordHashTest
    {
        [Test]
        public void VerifyPassword_ShouldReturn_TrueIfPasswordIsAMatch()
        {
            string unHashed = "aaaaaa";

            string hashedPass = HashThePassword(unHashed, null, false);

            Assert.True(VerifyThePassword(unHashed, hashedPass));
        }

        [Test]
        public void VerifyPassword_ShouldReturn_FalseIfPasswordIsNotAMatch()
        {
            string unHashed = "aaaaaa";
            string unHashedWrong = unHashed + "a";

            string hashedPass = HashThePassword(unHashed, null, false);

            Assert.False(VerifyThePassword(unHashedWrong, hashedPass));
        }
    }
}