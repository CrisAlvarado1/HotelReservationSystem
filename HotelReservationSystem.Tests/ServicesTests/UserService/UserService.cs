using System.ComponentModel.DataAnnotations;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Infrastructure.Interfaces;
using Moq;
using HotelReservationSystem.Core.Services;
using HotelReservationSystem.Infrastructure.Data.Enum;

namespace HotelReservationSystem.Tests.ServicesTests
{
    [TestFixture]
    public class UserServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private UserService _userService;

        [SetUp]
        public void Setup()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userService = new UserService(_userRepositoryMock.Object);
        }

        /// <summary>
        /// TC-RH-001 - Test to verify that an ArgumentNullException is thrown when the user is null.
        /// </summary>
        [Test]
        public void RegisterUserAsync_NullUser_ThrowsArgumentNullException()
        {
            User nullUser = null;
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _userService.RegisterUserAsync(nullUser));
            Assert.That(ex.Message, Does.Contain("The user cannot be null."));
        }

        /// <summary>
        /// TC-RH-002 - Test to verify that a ValidationException is thrown when the user name is empty.
        /// </summary>
        [Test]
        [TestCase("")]
        [TestCase("   ")]
        public void RegisterUserAsync_EmptyName_ThrowsValidationException(string invalidName)
        {
            var user = new User { Name = invalidName, LastName = "Madrigal", Email = "example@example.com", PhoneNumber = "1234567890", UserType = UserType.Client };
            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _userService.RegisterUserAsync(user));
            Assert.That(ex.Message, Is.EqualTo("The user name is required."));
        }

        /// <summary>
        /// TC-RH-003 - Test to verify that a ValidationException is thrown when the user last name is empty.
        /// </summary>
        [Test]
        [TestCase("")]
        [TestCase("   ")]
        public void RegisterUserAsync_EmptyLastName_ThrowsValidationException(string invalidLastName)
        {
            var user = new User { Name = "Michelle", LastName = invalidLastName, Email = "example@example.com", PhoneNumber = "1234567890", UserType = UserType.Client };
            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _userService.RegisterUserAsync(user));
            Assert.That(ex.Message, Is.EqualTo("The user last name is required."));
        }

        /// <summary>
        /// TC-RH-004 - Test to verify that a ValidationException is thrown when the user email is empty.
        /// </summary>
        [Test]
        [TestCase("")]
        [TestCase("   ")]
        public void RegisterUserAsync_EmptyEmail_ThrowsValidationException(string invalidEmail)
        {
            var user = new User { Name = "Michelle", LastName = "Madrigal", Email = invalidEmail, PhoneNumber = "1234567890", UserType = UserType.Client };
            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _userService.RegisterUserAsync(user));
            Assert.That(ex.Message, Is.EqualTo("The user email is required."));
        }

        /// <summary>
        /// TC-RH-005 - Test to verify that a ValidationException is thrown when the email is invalid.
        /// </summary>
        [Test]
        [TestCase("invalid-email")]
        [TestCase("user@invalid")]
        public void RegisterUserAsync_InvalidEmail_ThrowsValidationException(string invalidEmail)
        {
            var user = new User { Name = "Michelle", LastName = "Madrigal", Email = invalidEmail, PhoneNumber = "1234567890", UserType = UserType.Client };
            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _userService.RegisterUserAsync(user));
            Assert.That(ex.Message, Is.EqualTo("The user email is not valid."));
        }

        /// <summary>
        /// TC-RH-006 - Test to verify that the AddAsync method is called when registering a valid user.
        /// </summary>
        [Test]
        public async Task RegisterUserAsync_ValidUser_CallsAddAsync()
        {
            var user = new User { Name = "Michelle", LastName = "Madrigal", Email = "michelle.doe@example.com", PhoneNumber = "1234567890", UserType = UserType.Client };
            _userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<User>()))
                               .ReturnsAsync(user);
            var result = await _userService.RegisterUserAsync(user);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo(user.Name));
            Assert.That(result.LastName, Is.EqualTo(user.LastName));
            Assert.That(result.Email, Is.EqualTo(user.Email));
            _userRepositoryMock.Verify(repo => repo.AddAsync(It.Is<User>(u => u.Email == "michelle.doe@example.com")), Times.Once);
        }
    }
}
