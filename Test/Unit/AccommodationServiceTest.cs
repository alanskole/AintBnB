using AintBnB.BusinessLogic.CustomExceptions;
using AintBnB.BusinessLogic.Imp;
using AintBnB.Core.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static AintBnB.BusinessLogic.Helpers.Authentication;

namespace Test.Unit
{
    [TestFixture]
    public class AccommodationServiceTest : TestBase
    {
        [SetUp]
        public void SetUp()
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
        public void CreateAccommodation_ShouldFail_IfDaysToCreateScheduleForIsLessThanOne()
        {
            LoggedInAs = userCustomer1;

            var ex = Assert.Throws<ParameterException>(()
                => accommodationService.CreateAccommodation(userCustomer2, adr, 50, 2, 2.3, "mmm mmm", 600, 2, new List<byte[]>(), 0));

            Assert.AreEqual("Days to create the schedule for cannot be less than one!", ex.Message);
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

        [Test]
        [TestCase(0, "ss", "3f", "23", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "", "3f", "23", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "ss", "s", "23", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "ss", "3f", "", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "ss", "3f", "23", "", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "ss", "3f", "23", "ss", "Fredrikstad", "Norway", 0, "ss", 10, 1)]
        [TestCase(1, "ss", "3f", "23", "ss", "Fredrikstad", "Norway", 10, "", 10, 1)]
        [TestCase(1, "ss", "3f", "23", "ss", "Fredrikstad", "Norway", 10, "ss", 0, 1)]
        [TestCase(1, "ss", "3f", "23", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 0)]
        public void ValidateAccommodation_ShouldFail_IfNotAllCriteriasMet(int ownerId, string street, string number, string zip, string area, string city, string country, int squareMeters, string description, int pricePerNight, int cancellationDeadlineInDays)
        {
            User owner = new User { Id = ownerId };
            Address ad = new Address { Street = street, Number = number, Zip = zip, Area = area, City = city, Country = country };
            Accommodation acc = new Accommodation { Owner = owner, Address = ad, SquareMeters = squareMeters, Description = description, PricePerNight = pricePerNight, CancellationDeadlineInDays = cancellationDeadlineInDays };

            var ex = Assert.Throws<ParameterException>(()
                => accommodationService.ValidateAccommodation(acc));
        }

        [Test]
        [TestCase(1, "ss", "3f", "23", "ss", "London", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "ss", "3f", "23", "ss", "Osloo", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "ss", "3f", "23", "ss", "Oslo", "Norrway", 10, "ss", 10, 1)]
        public void ValidateAccommodation_ShouldFail_IfACityIsNotInACountry_OrCityOrCountryDoesNotExist(int ownerId, string street, string number, string zip, string area, string city, string country, int squareMeters, string description, int pricePerNight, int cancellationDeadlineInDays)
        {
            User owner = new User { Id = ownerId };
            Address ad = new Address { Street = street, Number = number, Zip = zip, Area = area, City = city, Country = country };
            Accommodation acc = new Accommodation { Owner = owner, Address = ad, SquareMeters = squareMeters, Description = description, PricePerNight = pricePerNight, CancellationDeadlineInDays = cancellationDeadlineInDays };

            var ex = Assert.Throws<GeographicalException>(()
                => accommodationService.ValidateAccommodation(acc));
        }

        [Test]
        [TestCase(1, "s", "1s", "23", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "s--", "1s", "23", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "ss", "s", "23", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "ss", "s1", "23", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "ss", "1-", "23", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "ss", "1", "3", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "ss", "1", "123456789ab", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "ss", "1", "2-", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        public void ValidateAccommodation_ShouldFail_IfRegexCheckerFailsOnStreetNumberZipOrArea(int ownerId, string street, string number, string zip, string area, string city, string country, int squareMeters, string description, int pricePerNight, int cancellationDeadlineInDays)
        {
            User owner = new User { Id = ownerId };
            Address ad = new Address { Street = street, Number = number, Zip = zip, Area = area, City = city, Country = country };
            Accommodation acc = new Accommodation { Owner = owner, Address = ad, SquareMeters = squareMeters, Description = description, PricePerNight = pricePerNight, CancellationDeadlineInDays = cancellationDeadlineInDays };

            var ex = Assert.Throws<ParameterException>(()
                => accommodationService.ValidateAccommodation(acc));
        }

        [Test]
        [TestCase(1, "s-s", "1s", "23", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "1-s", "1s", "23", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "1-s-2-s", "1s", "23", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "1 s 2 s", "1s", "23", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "1 s-2 s", "1s", "23", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "ss", "1s", "23", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        public void ValidateAccommodation_ShouldSucceed_IfStreetIsLongerThan2Characters_WithOnlyLettersNumbers_WithOneSpaceOrDashBetweenThem(int ownerId, string street, string number, string zip, string area, string city, string country, int squareMeters, string description, int pricePerNight, int cancellationDeadlineInDays)
        {
            User owner = new User { Id = ownerId };
            Address ad = new Address { Street = street, Number = number, Zip = zip, Area = area, City = city, Country = country };
            Accommodation acc = new Accommodation { Owner = owner, Address = ad, SquareMeters = squareMeters, Description = description, PricePerNight = pricePerNight, CancellationDeadlineInDays = cancellationDeadlineInDays };

            Assert.DoesNotThrow(() => accommodationService.ValidateAccommodation(acc));
        }

        [Test]
        [TestCase(1, "s-s", "123", "23", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "s-s", "2", "23", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        public void ValidateAccommodation_ShouldSucceed_IfNumberStartsWithNumberLargerThan0_WithOneOptionalLetterAtTheEnd(int ownerId, string street, string number, string zip, string area, string city, string country, int squareMeters, string description, int pricePerNight, int cancellationDeadlineInDays)
        {
            User owner = new User { Id = ownerId };
            Address ad = new Address { Street = street, Number = number, Zip = zip, Area = area, City = city, Country = country };
            Accommodation acc = new Accommodation { Owner = owner, Address = ad, SquareMeters = squareMeters, Description = description, PricePerNight = pricePerNight, CancellationDeadlineInDays = cancellationDeadlineInDays };

            Assert.DoesNotThrow(() => accommodationService.ValidateAccommodation(acc));
        }

        [Test]
        [TestCase(1, "s-s", "2", "12345-6789", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "s-s", "2", "abcd-6789", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "s-s", "2", "ab", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "s-s", "2", "1-2", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "s-s", "2", "1 2-s", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        [TestCase(1, "s-s", "2", "1 2 s", "ss", "Fredrikstad", "Norway", 10, "ss", 10, 1)]
        public void ValidateAccommodation_ShouldSucceed_IfZipIsBetween2And10Characters_WithOnlyLettersAndNumbersWithSpaceOrDashBetweenThem(int ownerId, string street, string number, string zip, string area, string city, string country, int squareMeters, string description, int pricePerNight, int cancellationDeadlineInDays)
        {
            User owner = new User { Id = ownerId };
            Address ad = new Address { Street = street, Number = number, Zip = zip, Area = area, City = city, Country = country };
            Accommodation acc = new Accommodation { Owner = owner, Address = ad, SquareMeters = squareMeters, Description = description, PricePerNight = pricePerNight, CancellationDeadlineInDays = cancellationDeadlineInDays };

            Assert.DoesNotThrow(() => accommodationService.ValidateAccommodation(acc));
        }

        [Test]
        public void CreateScheduleForXAmountOfDays_Should_CreateScheduleForCorrectAmountOfDays()
        {
            Accommodation acc = new Accommodation();

            int days = 100;

            Assert.Null(acc.Schedule);

            var result = typeof(AccommodationService)
                            .GetMethod("CreateScheduleForXAmountOfDays", BindingFlags.NonPublic | BindingFlags.Instance);
            result.Invoke(accommodationService, new object[] { acc, days });

            Assert.AreEqual(acc.Schedule.Count, days);
        }

        [Test]
        public void AddDaysToDateAndAddToSchedule_Shoudl_AddCorrectDatesToSchedule()
        {
            SortedDictionary<string, bool> dateAndStatus = new SortedDictionary<string, bool>();

            DateTime td = DateTime.Today;

            int daysToAddToSchedule = 10;

            Assert.AreEqual(0, dateAndStatus.Count);

            var result = typeof(AccommodationService)
                            .GetMethod("AddDaysToDateAndAddToSchedule", BindingFlags.NonPublic | BindingFlags.Instance);
            result.Invoke(accommodationService, new object[] { daysToAddToSchedule, td, dateAndStatus });

            Assert.AreEqual(td.ToString("yyyy-MM-dd"), dateAndStatus.Keys.First());

            Assert.AreEqual(td.AddDays(daysToAddToSchedule - 1).ToString("yyyy-MM-dd"), dateAndStatus.Keys.Last());
        }

        [Test]
        public void GetAccommodation_ShouldReturn_CorrectAccommodationIfAnyoneLoggedIn()
        {
            LoggedInAs = userCustomer2;

            CreateDummyAccommodation();

            Assert.AreEqual(accommodation1.Id, accommodationService.GetAccommodation(1).Id);
            Assert.AreEqual(accommodation1.Owner.Id, accommodationService.GetAccommodation(1).Owner.Id);
            Assert.AreEqual(accommodation1.Address.Street, accommodationService.GetAccommodation(1).Address.Street);
            Assert.AreEqual(accommodation1.ToString(), accommodationService.GetAccommodation(1).ToString());
        }

        [Test]
        public void GetAccommodation_ShouldFail_IfNoOneLoggedIn()
        {
            LoggedInAs = null;

            CreateDummyAccommodation();

            var ex = Assert.Throws<LoginException>(()
                => accommodationService.GetAccommodation(1));

            Assert.AreEqual("Not logged in!", ex.Message);
        }

        [Test]
        public void GetAccommodation_ShouldFail_IfNoAccommodationsWithTheIdExists()
        {
            LoggedInAs = userAdmin;

            CreateDummyAccommodation();

            var ex = Assert.Throws<IdNotFoundException>(()
                => accommodationService.GetAccommodation(1000));

            Assert.AreEqual("Accommodation with ID 1000 not found!", ex.Message);
        }

        [Test]
        public void GetAllAccommodations_ShouldReturn_CorrectAccommodationIfAnyoneLoggedInAndDatabaseHasAccommodations()
        {
            LoggedInAs = userCustomer2;

            CreateDummyAccommodation();

            Assert.AreEqual(3, accommodationService.GetAllAccommodations().Count);
        }

        [Test]
        public void GetAllAccommodations_ShouldFail_IfNoAccommodationsInDatabase()
        {
            LoggedInAs = userCustomer1;

            var ex = Assert.Throws<NoneFoundInDatabaseTableException>(()
                => accommodationService.GetAllAccommodations());

            Assert.AreEqual("No accommodations found!", ex.Message);
        }

        [Test]
        public void GetAllAccommodations_ShouldFail_IfNoOneLoggedIn()
        {
            LoggedInAs = null;

            CreateDummyAccommodation();

            var ex = Assert.Throws<LoginException>(()
                => accommodationService.GetAllAccommodations());

            Assert.AreEqual("Not logged in!", ex.Message);
        }

        [Test]
        public void GetAllOwnedAccommodations_ShouldReturn_AllTheAccommodationsOwnedByAUser()
        {
            LoggedInAs = userCustomer2;

            CreateDummyAccommodation();

            Assert.AreEqual(2, accommodationService.GetAllOwnedAccommodations(userCustomer2.Id).Count);
            Assert.AreEqual(userCustomer2.Id, accommodationService.GetAllOwnedAccommodations(userCustomer2.Id)[0].Owner.Id);
            Assert.AreEqual(userCustomer2.ToString(), accommodationService.GetAllOwnedAccommodations(userCustomer2.Id)[0].Owner.ToString());
            Assert.AreEqual(userCustomer2.Id, accommodationService.GetAllOwnedAccommodations(userCustomer2.Id)[1].Owner.Id);
            Assert.AreEqual(userCustomer2.ToString(), accommodationService.GetAllOwnedAccommodations(userCustomer2.Id)[1].Owner.ToString());
        }

        [Test]
        public void GetAllOwnedAccommodations_ShouldFail_IfTheUserHasNoAccommodations()
        {
            LoggedInAs = userAdmin;

            CreateDummyAccommodation();

            var ex = Assert.Throws<NoneFoundInDatabaseTableException>(()
                => accommodationService.GetAllOwnedAccommodations(1));

            Assert.AreEqual("User with Id 1 doesn't have any accommodations!", ex.Message);
        }

        [Test]
        public void UpdateAccommodation_ShouldSucceed_IfChangingSquareMetersDescriptionPricePerNightOrCancellationDeadlineInDays()
        {
            CreateDummyAccommodation();

            LoggedInAs = accommodation1.Owner;

            Accommodation upd = new Accommodation { SquareMeters = 1, Description = "new", PricePerNight = 1, CancellationDeadlineInDays = 11 };

            Assert.AreNotEqual(accommodation1.SquareMeters, upd.SquareMeters);
            Assert.AreNotEqual(accommodation1.Description, upd.Description);
            Assert.AreNotEqual(accommodation1.PricePerNight, upd.PricePerNight);
            Assert.AreNotEqual(accommodation1.CancellationDeadlineInDays, upd.CancellationDeadlineInDays);

            accommodationService.UpdateAccommodation(accommodation1.Id, upd);

            Assert.AreEqual(accommodation1.SquareMeters, upd.SquareMeters);
            Assert.AreEqual(accommodation1.Description, upd.Description);
            Assert.AreEqual(accommodation1.PricePerNight, upd.PricePerNight);
            Assert.AreEqual(accommodation1.CancellationDeadlineInDays, upd.CancellationDeadlineInDays);
        }

        [Test]
        public void UpdateAccommodation_ShouldFail_IfNotDoneByACustomerThatDoesNotOwnTheAccommodation()
        {
            CreateDummyAccommodation();

            LoggedInAs = userCustomer2;

            Accommodation upd = new Accommodation { SquareMeters = 1, Description = "new", PricePerNight = 1, CancellationDeadlineInDays = 11 };

            var ex = Assert.Throws<AccessException>(()
                => accommodationService.UpdateAccommodation(accommodation1.Id, upd));

            Assert.AreEqual($"Must be performed by a customer with ID {accommodation1.Owner.Id}, or by admin or an employee on behalf of a customer with ID {accommodation1.Owner.Id}!", ex.Message);
        }

        [Test]
        public void UpdateAccommodation_ShouldSucceed_IfDoneByEmployeeOnBehalfOfTheOwner()
        {
            CreateDummyAccommodation();

            LoggedInAs = accommodation1.Owner;

            Accommodation upd = new Accommodation { SquareMeters = 1, Description = "new", PricePerNight = 1, CancellationDeadlineInDays = 11 };

            Assert.AreNotEqual(accommodation1.SquareMeters, upd.SquareMeters);
            Assert.AreNotEqual(accommodation1.Description, upd.Description);
            Assert.AreNotEqual(accommodation1.PricePerNight, upd.PricePerNight);
            Assert.AreNotEqual(accommodation1.CancellationDeadlineInDays, upd.CancellationDeadlineInDays);

            accommodationService.UpdateAccommodation(accommodation1.Id, upd);

            Assert.AreEqual(accommodation1.SquareMeters, upd.SquareMeters);
            Assert.AreEqual(accommodation1.Description, upd.Description);
            Assert.AreEqual(accommodation1.PricePerNight, upd.PricePerNight);
            Assert.AreEqual(accommodation1.CancellationDeadlineInDays, upd.CancellationDeadlineInDays);
        }

        [Test]
        public void UpdateAccommodation_ShouldSucceed_IfDoneByAdminOnBehalfOfTheOwner()
        {
            LoggedInAs = userAdmin;

            CreateDummyAccommodation();

            Accommodation upd = new Accommodation { SquareMeters = 1, Description = "new", PricePerNight = 1, CancellationDeadlineInDays = 11 };

            Assert.AreNotEqual(accommodation1.SquareMeters, upd.SquareMeters);
            Assert.AreNotEqual(accommodation1.Description, upd.Description);
            Assert.AreNotEqual(accommodation1.PricePerNight, upd.PricePerNight);
            Assert.AreNotEqual(accommodation1.CancellationDeadlineInDays, upd.CancellationDeadlineInDays);

            accommodationService.UpdateAccommodation(accommodation1.Id, upd);

            Assert.AreEqual(accommodation1.SquareMeters, upd.SquareMeters);
            Assert.AreEqual(accommodation1.Description, upd.Description);
            Assert.AreEqual(accommodation1.PricePerNight, upd.PricePerNight);
            Assert.AreEqual(accommodation1.CancellationDeadlineInDays, upd.CancellationDeadlineInDays);
        }

        [Test]
        public void ExpandScheduleOfAccommodationWithXAmountOfDays_ShouldSucceed()
        {
            CreateDummyAccommodation();

            LoggedInAs = accommodation1.Owner;

            int exandByDays = 10;

            int newSize = accommodation1.Schedule.Count + exandByDays;

            Assert.AreNotEqual(accommodation1.Schedule.Count, newSize);

            accommodationService.ExpandScheduleOfAccommodationWithXAmountOfDays(accommodation1.Id, exandByDays);

            Assert.AreEqual(accommodation1.Schedule.Count, newSize);
        }

        [Test]
        public void ExpandScheduleOfAccommodationWithXAmountOfDays_ShouldFail_IfDoneByACustomerThatDoesNotOwnTheAccommodation()
        {
            CreateDummyAccommodation();

            LoggedInAs = accommodation3.Owner;

            var ex = Assert.Throws<AccessException>(()
                => accommodationService.ExpandScheduleOfAccommodationWithXAmountOfDays(accommodation1.Id, 10));

            Assert.AreEqual($"Must be performed by a customer with ID {accommodation1.Owner.Id}, or by admin or an employee on behalf of a customer with ID {accommodation1.Owner.Id}!", ex.Message);
        }

        [Test]
        public void ExpandScheduleOfAccommodationWithXAmountOfDays_ShouldFail_IfDaysToExapndByIsLessThanOne()
        {
            CreateDummyAccommodation();

            LoggedInAs = accommodation1.Owner;

            var ex = Assert.Throws<ParameterException>(()
                => accommodationService.ExpandScheduleOfAccommodationWithXAmountOfDays(accommodation1.Id, 0));

            Assert.AreEqual("Days cannot be less than one!", ex.Message);
        }

        [Test]
        public void ExpandScheduleOfAccommodationWithXAmountOfDays_ShouldFail_IfNoAccommodationWithTheIdExist()
        {
            CreateDummyAccommodation();

            LoggedInAs = accommodation1.Owner;

            var ex = Assert.Throws<IdNotFoundException>(()
                => accommodationService.ExpandScheduleOfAccommodationWithXAmountOfDays(100, 10));

            Assert.AreEqual("Accommodation with ID 100 not found!", ex.Message);
        }

        [Test]
        public void FindAvailable_ShouldReturn_AllAvailableAccommodationsInACityIfAvailable()
        {
            CreateDummyAccommodation();

            LoggedInAs = accommodation1.Owner;

            Assert.AreEqual(accommodation1.Address.City, accommodation2.Address.City);
            Assert.AreEqual(accommodation1.Address.Country, accommodation2.Address.Country);
            Assert.AreNotEqual(accommodation1.Address.City, accommodation3.Address.City);

            List<Accommodation> av = accommodationService.FindAvailable(accommodation1.Address.Country, accommodation1.Address.City, DateTime.Today.ToString("yyyy-MM-dd"), 5);

            Assert.True(av.Contains(accommodation1));
            Assert.True(av.Contains(accommodation2));
            Assert.False(av.Contains(accommodation3));
        }

        [Test]
        public void FindAvailable_ShouldFail_IfNoDatesAvailable()
        {
            CreateDummyAccommodation();

            CreateDummyBooking();

            LoggedInAs = accommodation1.Owner;

            var ex = Assert.Throws<DateException>(()
                => accommodationService.FindAvailable(accommodation1.Address.Country, accommodation1.Address.City, DateTime.Today.AddDays(6).ToString("yyyy-MM-dd"), 15));

            Assert.AreEqual($"No available accommodations found in {accommodation1.Address.Country}, {accommodation1.Address.City} from {DateTime.Today.AddDays(6).ToString("yyyy-MM-dd")} for {15} nights", ex.Message);
        }

        [Test]
        public void SortListOfAccommodations_ShouldReturn_ListWithCorrectSorting()
        {
            CreateDummyAccommodation();

            LoggedInAs = accommodation1.Owner;

            List<Accommodation> av = accommodationService.FindAvailable(accommodation1.Address.Country, accommodation1.Address.City, DateTime.Today.ToString("yyyy-MM-dd"), 5);

            Assert.True(accommodation1.SquareMeters < accommodation2.SquareMeters);

            Assert.AreEqual(accommodation1.Id, av[0].Id);

            Assert.AreEqual(accommodation2.Id, accommodationService.SortListOfAccommodations(av, "Size", "Descending")[0].Id);

            Assert.AreEqual(accommodation1.Id, accommodationService.SortListOfAccommodations(av, "Size", "Ascending")[0].Id);


            Assert.True(accommodation1.PricePerNight < accommodation2.PricePerNight);

            Assert.AreEqual(accommodation2.Id, accommodationService.SortListOfAccommodations(av, "Price", "Descending")[0].Id);

            Assert.AreEqual(accommodation1.Id, accommodationService.SortListOfAccommodations(av, "Price", "Ascending")[0].Id);


            Assert.True(accommodation1.KilometersFromCenter < accommodation2.KilometersFromCenter);

            Assert.AreEqual(accommodation2.Id, accommodationService.SortListOfAccommodations(av, "Distance", "Descending")[0].Id);

            Assert.AreEqual(accommodation1.Id, accommodationService.SortListOfAccommodations(av, "Distance", "Ascending")[0].Id);


            accommodation1.AmountOfRatings = 1;
            accommodation1.AverageRating = 2;
            accommodation2.AmountOfRatings = 1;
            accommodation2.AverageRating = 5;

            Assert.AreEqual(accommodation2.Id, accommodationService.SortListOfAccommodations(av, "Rating", "Descending")[0].Id);

            Assert.AreEqual(accommodation1.Id, accommodationService.SortListOfAccommodations(av, "Rating", "Ascending")[0].Id);
        }
    }
}