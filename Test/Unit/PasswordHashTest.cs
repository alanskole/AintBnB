using Microsoft.VisualStudio.TestTools.UnitTesting;
using static AintBnB.BusinessLogic.Helpers.PasswordHashing;

namespace Test.Unit
{
    [TestClass]
    public class PasswordHashTest
    {
        [TestMethod]
        public void VerifyPassword_ShouldReturn_TrueIfPasswordIsAMatch()
        {
            string unHashed = "aaaaaa";

            string hashedPass = HashThePassword(unHashed, null, false);

            Assert.IsTrue(VerifyThePassword(unHashed, hashedPass));
        }

        [TestMethod]
        public void VerifyPassword_ShouldReturn_FalseIfPasswordIsNotAMatch()
        {
            string unHashed = "aaaaaa";
            string unHashedWrong = unHashed + "a";

            string hashedPass = HashThePassword(unHashed, null, false);

            Assert.IsFalse(VerifyThePassword(unHashedWrong, hashedPass));
        }
    }
}