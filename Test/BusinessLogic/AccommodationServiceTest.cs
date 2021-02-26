using AintBnB.Core.Models;
using NUnit.Framework;
using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Imp;
using System.Reflection;
using System.Collections.Generic;
using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace Test.BusinessLogic
{
    [TestFixture]
    public class AccommodationServiceTest : TestBase
    {
        [SetUp]
        public void Setup()
        {
            SetupDatabaseForTesting();
            SetupTestClasses();
            CreateDummyUsers();
        }

        [TearDown]
        public void TearDown()
        {
            Dispose();
        }

        [Test]
        public void CreateAccommodation_ShouldReturn_NewAccommodationWhenCustomerCreatesOwnedByTheCustomer()
        {
            LoggedInAs = userCustomer1;

            Accommodation acc = accommodationService.CreateAccommodation(userCustomer1, adr, 50, 2, 2.3, "mmm mmm", 600, 2, new List<byte[]>(), 100);

            Assert.AreEqual(1, acc.Id);
            Assert.AreEqual(userCustomer1.Id, acc.Owner.Id);
            Assert.AreEqual(adr, acc.Address);
        }

        [Test]
        public void CreateAccommodation_ShouldReturn_NewAccommodationWhenAdminCreatesOnBehalfOfCustomer()
        {
            LoggedInAs = userAdmin;

            Accommodation acc = accommodationService.CreateAccommodation(userCustomer1, adr, 50, 2, 2.3, "mmm mmm", 600, 2, new List<byte[]>(), 100);

            Assert.AreEqual(1, acc.Id);
            Assert.AreEqual(userCustomer1.Id, acc.Owner.Id);
            Assert.AreEqual(adr, acc.Address);
        }

        [Test]
        public void CreateAccommodation_ShouldReturn_NewAccommodationWhenEmployeeCreatesOnBehalfOfCustomer()
        {
            LoggedInAs = userEmployee1;

            Accommodation acc = accommodationService.CreateAccommodation(userCustomer1, adr, 50, 2, 2.3, "mmm mmm", 600, 2, new List<byte[]>(), 100);

            Assert.AreEqual(1, acc.Id);
            Assert.AreEqual(userCustomer1.Id, acc.Owner.Id);
            Assert.AreEqual(adr, acc.Address);
        }

        [Test]
        public void CreateAccommodation_ShouldFail_IfOwnerIsNotCustomer()
        {
            LoggedInAs = userAdmin;

            var ex = Assert.Throws<AccessException>(()
                => accommodationService.CreateAccommodation(userAdmin, adr, 50, 2, 2.3, "mmm mmm", 600, 2, new List<byte[]>(), 100));

            Assert.AreEqual(ex.Message, $"Must be performed by a customer with ID {userAdmin.Id}, or by admin or an employee on behalf of a customer with ID {userAdmin.Id}!");

            LoggedInAs = userEmployee1;

            ex = Assert.Throws<AccessException>(()
                => accommodationService.CreateAccommodation(userEmployee1, adr2, 50, 2, 2.3, "mmm mmm", 600, 2, new List<byte[]>(), 100));

            Assert.AreEqual(ex.Message, $"Must be performed by a customer with ID {userEmployee1.Id}, or by admin or an employee on behalf of a customer with ID {userEmployee1.Id}!");
        }

        [Test]
        public void CreateAccommodation_ShouldFail_IfCustomerTriesToCreateAccommodationForAnotherUser()
        {
            LoggedInAs = userCustomer1;

            var ex = Assert.Throws<AccessException>(()
                => accommodationService.CreateAccommodation(userCustomer2, adr, 50, 2, 2.3, "mmm mmm", 600, 2, new List<byte[]>(), 100));

            Assert.AreEqual(ex.Message, $"Must be performed by a customer with ID {userCustomer2.Id}, or by admin or an employee on behalf of a customer with ID {userCustomer2.Id}!");
        }
    }
}
