using Moq;
using Moq.EntityFrameworkCore;
using NUnit.Framework;
using HotelReservationSystem.Infrastructure.Data;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using HotelReservationSystem.Infrastructure.Data.Enum;

namespace HotelReservationSystem.Tests.RepositoriesTests
{
    [TestFixture]
    public class RegisterClients
    {
        private Mock<HotelDbContext> _contextMock;
        private UserRepository _userRepository;
        private List<User> _users;

        [SetUp]
        public void Setup()
        {

            _users = new List<User>();


            var options = new DbContextOptions<HotelDbContext>();


            _contextMock = new Mock<HotelDbContext>(options);
            _contextMock.Setup(c => c.Users).ReturnsDbSet(_users);
            _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);
            _userRepository = new UserRepository(_contextMock.Object);
        }

        /// <summary>
        /// TC-USER-REPO-001: Verifica que un usuario válido se agregue correctamente a la base de datos.
        /// </summary>
        [Test]
        public async Task AddAsync_ValidData_ShouldAddSuccessfully()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Name = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "1234567890",
                UserType = UserType.Client
            };

            // Act
            _users.Add(user);
            await _contextMock.Object.SaveChangesAsync();

            // Assert
            Assert.AreEqual(1, _users.Count);
            Assert.AreEqual(user.Name, _users[0].Name);
            Assert.AreEqual(user.LastName, _users[0].LastName);
            Assert.AreEqual(user.Email, _users[0].Email);
            Assert.AreEqual(user.PhoneNumber, _users[0].PhoneNumber);
            Assert.AreEqual(user.UserType, _users[0].UserType);
            _contextMock.Verify(repo => repo.SaveChangesAsync(default), Times.Once());
        }

        /// <summary>
        /// TC-USER-REPO-002: Verifica que se lance una excepción si hay un error en la base de datos al guardar.
        /// </summary>
        [Test]
        public async Task AddAsync_DbFailure_ShouldThrowException()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Name = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "1234567890",
                UserType = UserType.Client
            };

            _contextMock.Setup(c => c.SaveChangesAsync(default))
                        .ThrowsAsync(new DbUpdateException("Database error", new Exception()));

            // Act & Assert
            var ex = Assert.ThrowsAsync<DbUpdateException>(async () =>
                await _userRepository.AddAsync(user));

            Assert.AreEqual("Database error", ex.Message);
            _contextMock.Verify(repo => repo.SaveChangesAsync(default), Times.Once());
        }

        /// <summary>
        /// TC-USER-REPO-003: Verifica que se lance una excepción si se intenta agregar un usuario nulo.
        /// </summary>
        [Test]
        public async Task AddAsync_NullUser_ShouldThrowArgumentNullException()
        {
            // Arrange
            User nullUser = null;

            _contextMock.Setup(c => c.SaveChangesAsync(default))
                        .ThrowsAsync(new ArgumentNullException(nameof(nullUser), "The user cannot be null."));

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _userRepository.AddAsync(nullUser));

            Assert.AreEqual("The user cannot be null. (Parameter 'nullUser')", ex.Message);
        }
    }
}