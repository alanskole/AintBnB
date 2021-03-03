using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using static AintBnB.BusinessLogic.Helpers.PasswordHashing;
using AintBnB.BusinessLogic.Helpers;
using System.Reflection;

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