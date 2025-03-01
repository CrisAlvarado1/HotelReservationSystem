using HotelReservationSystem.Core.Services;
using HotelReservationSystem.Infrastructure.Interfaces;
using HotelReservationSystem.Infrastructure.Models;
using Moq;

namespace HotelReservationSystem.Tests.ServicesTests
{
    [TestFixture]
    public class ReservationHistoryServiceTests
    {
        private Mock<IReservationRepository> _reservationRepositoryMock;
        private Mock<IRoomRepository> _roomRepositoryMock;
        private ReservationService _reservationService;

        [SetUp]
        public void Setup()
        {
            _reservationRepositoryMock = new Mock<IReservationRepository>();
            _roomRepositoryMock = new Mock<IRoomRepository>();
            _reservationService = new ReservationService(_reservationRepositoryMock.Object, _roomRepositoryMock.Object);
        }

        /// <summary>
        /// TC-RH-001 - Verifies that a list of reservations is returned for a valid user with registered reservations.
        /// </summary>
        [Test]
        public async Task GetUserReservationHistory_ValidUserId_ShouldReturnReservations()
        {
            int userId = 1;
            var reservations = new List<Reservation>
            {
                new Reservation {Id = 1, ClientId = userId, RoomId = 101},
                new Reservation {Id = 2, ClientId = userId, RoomId = 102}
            };
            _reservationRepositoryMock.Setup(repo => repo.GetUserReservationHistoryAsync(userId))
                .ReturnsAsync(reservations.AsEnumerable());
        }

        /// <summary>
        /// TC-RH-002 - Verifies that a user with no reservations triggers a KeyNotFoundException.
        /// </summary>
        [Test]
        public void GetUserReservationHistory_UserWithNoReservations_ShouldThrowKeyNotFoundException()
        {
            int userId = 2;
            _reservationRepositoryMock.Setup(repo => repo.GetUserReservationHistoryAsync(userId))
                .ReturnsAsync(Enumerable.Empty<Reservation>());

            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _reservationService.ReservationHistoryAsync(userId));
            Assert.AreEqual($"No reservations found for user ID {userId}.", ex.Message);
        }

        /// <summary>
        /// TC-RH-003 - Verifies that an invalid user ID (0 or negative) triggers an ArgumentException.
        /// </summary>
        [Test]
        public void GetUserReservationHistory_InvalidUserId_ShouldThrowArgumentException()
        {
            int invalidUserId = -1;
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _reservationService.ReservationHistoryAsync(invalidUserId));
            Assert.AreEqual("User ID must be greater than zero. (Parameter 'clienteId')", ex.Message);
        }

        /// <summary>
        /// TC-RH-004 - Verifies that an unregistered user triggers a KeyNotFoundException.
        /// </summary>
        [Test]
        public void GetUserReservationHistory_NonExistentUser_ShouldThrowKeyNotFoundException()
        {
            int nonExistentUserId = 999;
            _reservationRepositoryMock.Setup(repo => repo.GetUserReservationHistoryAsync(nonExistentUserId))
                .ReturnsAsync(Enumerable.Empty<Reservation>());

            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _reservationService.ReservationHistoryAsync(nonExistentUserId));
            Assert.AreEqual($"No reservations found for user ID {nonExistentUserId}.", ex.Message);
        }
    }
}
