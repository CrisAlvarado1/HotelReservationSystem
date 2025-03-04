using Moq;
using HotelReservationSystem.Core.Services;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Infrastructure.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem.Tests.ServicesTests
{
    [TestFixture]
    public class RegisterReservation
    {
        private Mock<IRoomRepository> _roomRepositoryMock;
        private Mock<IReservationRepository> _reservationRepositoryMock;
        private ReservationService _reservationService;

        [SetUp]
        public void Setup()
        {
            _roomRepositoryMock = new Mock<IRoomRepository>();
            _reservationRepositoryMock = new Mock<IReservationRepository>();
            _reservationService = new ReservationService(_reservationRepositoryMock.Object, _roomRepositoryMock.Object);
        }

        /// <summary>
        /// TC-RES-001: Verifies that a valid reservation is created correctly and updates the room status.
        /// </summary>
        [Test]
        public async Task ReserveRoom_ValidData_ShouldReserveSuccessfully()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = 1,
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.Now.AddDays(1), // Future date
                EndDate = DateTime.Now.AddDays(3),   // Date after StartDate
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };

            _roomRepositoryMock.Setup(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate))
                               .ReturnsAsync(true); 
            _reservationRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Reservation>()))
                                     .ReturnsAsync(reservation);

            // Act
            var result = await _reservationService.ReserveRoomAsync(reservation);

            // Assert
            Assert.IsNotNull(result); // We verify that the result is not null
            Assert.AreEqual(reservation.ClientId, result.ClientId); // Correct client
            Assert.AreEqual(reservation.RoomId, result.RoomId); // Correct room
            Assert.AreEqual(Infrastructure.Data.Enum.ReservationStatus.Confirmed, result.Status); // Confirmed status

            _roomRepositoryMock.Verify(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate), Times.Once); // Availability is checked
            _reservationRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Reservation>(r => r.RoomId == 1)), Times.Once); // The reservation is added
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(reservation.RoomId, false), Times.Once); // The room status is updated
        }

        /// <summary>
        /// TC-RES-002: Verifies that an exception is thrown if the room is not available.
        /// </summary>
        [Test]
        public async Task ReserveRoom_RoomNotAvailable_ShouldThrowException()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = 1,
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };

            _roomRepositoryMock.Setup(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate))
                               .ReturnsAsync(false); // The room is not available

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _reservationService.ReserveRoomAsync(reservation));
            Assert.AreEqual("The room is not available for the selected dates.", ex.Message);

            _roomRepositoryMock.Verify(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate), Times.Once); // Availability is checked
            _reservationRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Reservation>()), Times.Never); // The reservation is not added
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never); // The status is not updated
        }

        /// <summary>
        /// TC-RES-003: Verifies that an exception is thrown if the start date is in the past.
        /// </summary>
        [Test]
        public async Task ReserveRoom_PastStartDate_ShouldThrowArgumentException()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = 1,
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.Now.AddDays(-1), // Past date
                EndDate = DateTime.Now.AddDays(3),
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _reservationService.ReserveRoomAsync(reservation));
            Assert.AreEqual("The start date cannot be in the past.", ex.Message);

            _roomRepositoryMock.Verify(repo => repo.IsRoomAvailable(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never); // Availability is not checked
            _reservationRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Reservation>()), Times.Never); // The reservation is not added
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never); // The status is not updated
        }

        /// <summary>
        /// TC-RES-004: Verifies that an exception is thrown if the reservation is null.
        /// </summary>
        [Test]
        public async Task ReserveRoom_NullReservation_ShouldThrowArgumentNullException()
        {
            // Arrange
            Reservation reservation = null;

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _reservationService.ReserveRoomAsync(reservation));
            Assert.AreEqual("The reservation cannot be null. (Parameter 'reservation')", ex.Message);

            _roomRepositoryMock.Verify(repo => repo.IsRoomAvailable(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never); // Availability is not checked
            _reservationRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Reservation>()), Times.Never); // The reservation is not added
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never); // The status is not updated
        }

        /// <summary>
        /// TC-RES-005: Verifies that an exception is thrown if the end date is earlier than the start date.
        /// </summary>
        [Test]
        public async Task ReserveRoom_EndDateBeforeStartDate_ShouldThrowArgumentException()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = 1,
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.Now.AddDays(3), // Start date later than end date
                EndDate = DateTime.Now.AddDays(1), 
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _reservationService.ReserveRoomAsync(reservation));
            Assert.AreEqual("The start date must be earlier than the end date.", ex.Message);

            _roomRepositoryMock.Verify(repo => repo.IsRoomAvailable(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never); // Availability is not checked
            _reservationRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Reservation>()), Times.Never); // The reservation is not added
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never); // The status is not updated
        }

        /// <summary>
        /// TC-RES-006: Verifies that an exception is thrown if the room does not exist.
        /// </summary>
        [Test]
        public async Task ReserveRoom_RoomDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = 1,
                ClientId = 1,
                RoomId = 999, // Room ID that does not exist
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };

            _roomRepositoryMock.Setup(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate))
                               .ReturnsAsync(false); // We simulate that the room does not exist or is not available

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _reservationService.ReserveRoomAsync(reservation));
            Assert.AreEqual("The room is not available for the selected dates.", ex.Message);

            _roomRepositoryMock.Verify(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate), Times.Once); // Availability is checked
            _reservationRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Reservation>()), Times.Never); // The reservation is not added
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never); // The status is not updated
        }

        /// <summary>
        /// TC-RES-007: Verifies that the room status is not updated if the reservation fails due to overlapping dates.
        /// </summary>
        [Test]
        public async Task ReserveRoom_OverlappingDates_ShouldNotUpdateRoomAvailability()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = 1,
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };

            _roomRepositoryMock.Setup(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate))
                               .ReturnsAsync(false); // We simulate that there is an overlap

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _reservationService.ReserveRoomAsync(reservation));
            Assert.AreEqual("The room is not available for the selected dates.", ex.Message);

            _roomRepositoryMock.Verify(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate), Times.Once); // Availability is checked
            _reservationRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Reservation>()), Times.Never); // The reservation is not added
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never); // The status is not updated
        }

        /// <summary>
        /// TC-RES-008: Verifies that an exception is thrown if RoomId is invalid (e.g., 0).
        /// </summary>
        [Test]
        public async Task ReserveRoom_InvalidRoomId_ShouldThrowValidationException()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = 1,
                ClientId = 1,
                RoomId = 0, // Invalid RoomId
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };

            _roomRepositoryMock.Setup(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate))
                               .ReturnsAsync(true); // We simulate that the availability check passes
            _reservationRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Reservation>()))
                                     .ThrowsAsync(new ValidationException("RoomId must be greater than zero.")); // We simulate that EF Core throws an exception

            // Act & Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _reservationService.ReserveRoomAsync(reservation));
            Assert.AreEqual("RoomId must be greater than zero.", ex.Message);

            _roomRepositoryMock.Verify(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate), Times.Once); // Availability is checked
            _reservationRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Reservation>(r => r.RoomId == 0)), Times.Once); // Attempts to add the reservation
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never); // The status is not updated because it fails earlier
        }
    }
}