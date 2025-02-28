using Moq;
using HotelReservationSystem.Core.Services;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Infrastructure.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem.Tests
{
    [TestFixture]
    public class ReservationServiceTests
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
        /// TC-RES-001: Verifica que una reserva válida se crea correctamente y actualiza el estado de la habitación.
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
                StartDate = DateTime.Now.AddDays(1), // Fecha futura
                EndDate = DateTime.Now.AddDays(3),   // Fecha posterior a StartDate
                Status = HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };

            _roomRepositoryMock.Setup(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate))
                               .ReturnsAsync(true); // La habitación está disponible
            _reservationRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Reservation>()))
                                     .ReturnsAsync(reservation); // Simulamos que se agrega la reserva

            // Act
            var result = await _reservationService.ReserveRoomAsync(reservation);

            // Assert
            Assert.IsNotNull(result); // Verificamos que el resultado no sea nulo
            Assert.AreEqual(reservation.ClientId, result.ClientId); // Cliente correcto
            Assert.AreEqual(reservation.RoomId, result.RoomId); // Habitación correcta
            Assert.AreEqual(HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Confirmed, result.Status); // Estado confirmado

            _roomRepositoryMock.Verify(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate), Times.Once); // Se verifica disponibilidad
            _reservationRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Reservation>(r => r.RoomId == 1)), Times.Once); // Se agrega la reserva
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(reservation.RoomId, false), Times.Once); // Se actualiza el estado de la habitación
        }

        /// <summary>
        /// TC-RES-002: Verifica que se lanza una excepción si la habitación no está disponible.
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
                Status = HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };

            _roomRepositoryMock.Setup(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate))
                               .ReturnsAsync(false); // La habitación no está disponible

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _reservationService.ReserveRoomAsync(reservation));
            Assert.AreEqual("The room is not available for the selected dates.", ex.Message);

            _roomRepositoryMock.Verify(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate), Times.Once); // Se verifica disponibilidad
            _reservationRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Reservation>()), Times.Never); // No se agrega la reserva
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never); // No se actualiza el estado
        }

        /// <summary>
        /// TC-RES-003: Verifica que se lanza una excepción si la fecha de inicio está en el pasado.
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
                StartDate = DateTime.Now.AddDays(-1), // Fecha en el pasado
                EndDate = DateTime.Now.AddDays(3),
                Status = HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _reservationService.ReserveRoomAsync(reservation));
            Assert.AreEqual("The start date cannot be in the past.", ex.Message);

            _roomRepositoryMock.Verify(repo => repo.IsRoomAvailable(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never); // No se verifica disponibilidad
            _reservationRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Reservation>()), Times.Never); // No se agrega la reserva
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never); // No se actualiza el estado
        }

        /// <summary>
        /// TC-RES-004: Verifica que se lanza una excepción si la reserva es nula.
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

            _roomRepositoryMock.Verify(repo => repo.IsRoomAvailable(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never); // No se verifica disponibilidad
            _reservationRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Reservation>()), Times.Never); // No se agrega la reserva
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never); // No se actualiza el estado
        }

        /// <summary>
        /// TC-RES-005: Verifica que se lanza una excepción si la fecha de fin es anterior a la de inicio.
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
                StartDate = DateTime.Now.AddDays(3), // Fecha de inicio posterior a la de fin
                EndDate = DateTime.Now.AddDays(1),   // Fecha de fin anterior
                Status = HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _reservationService.ReserveRoomAsync(reservation));
            Assert.AreEqual("The start date must be earlier than the end date.", ex.Message);

            _roomRepositoryMock.Verify(repo => repo.IsRoomAvailable(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never); // No se verifica disponibilidad
            _reservationRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Reservation>()), Times.Never); // No se agrega la reserva
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never); // No se actualiza el estado
        }

        /// <summary>
        /// TC-RES-006: Verifica que se lanza una excepción si la habitación no existe.
        /// </summary>
        [Test]
        public async Task ReserveRoom_RoomDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = 1,
                ClientId = 1,
                RoomId = 999, // ID de habitación que no existe
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Status = HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };

            _roomRepositoryMock.Setup(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate))
                               .ReturnsAsync(false); // Simulamos que la habitación no existe o no está disponible

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _reservationService.ReserveRoomAsync(reservation));
            Assert.AreEqual("The room is not available for the selected dates.", ex.Message);

            _roomRepositoryMock.Verify(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate), Times.Once); // Se verifica disponibilidad
            _reservationRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Reservation>()), Times.Never); // No se agrega la reserva
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never); // No se actualiza el estado
        }

        /// <summary>
        /// TC-RES-007: Verifica que no se actualiza el estado de la habitación si la reserva falla por fechas solapadas.
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
                Status = HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };

            _roomRepositoryMock.Setup(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate))
                               .ReturnsAsync(false); // Simulamos que hay un solapamiento

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _reservationService.ReserveRoomAsync(reservation));
            Assert.AreEqual("The room is not available for the selected dates.", ex.Message);

            _roomRepositoryMock.Verify(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate), Times.Once); // Se verifica disponibilidad
            _reservationRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Reservation>()), Times.Never); // No se agrega la reserva
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never); // No se actualiza el estado
        }

        /// <summary>
        /// TC-RES-008: Verifica que se lanza una excepción si RoomId es inválido (por ejemplo, 0).
        /// </summary>
        [Test]
        public async Task ReserveRoom_InvalidRoomId_ShouldThrowValidationException()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = 1,
                ClientId = 1,
                RoomId = 0, // RoomId inválido
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Status = HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };

            _roomRepositoryMock.Setup(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate))
                               .ReturnsAsync(true); // Simulamos que la validación de disponibilidad pasa
            _reservationRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Reservation>()))
                                     .ThrowsAsync(new ValidationException("RoomId must be greater than zero.")); // Simulamos que EF Core lanza una excepción

            // Act & Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _reservationService.ReserveRoomAsync(reservation));
            Assert.AreEqual("RoomId must be greater than zero.", ex.Message);

            _roomRepositoryMock.Verify(repo => repo.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate), Times.Once); // Se verifica disponibilidad
            _reservationRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Reservation>(r => r.RoomId == 0)), Times.Once); // Intenta agregar la reserva
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never); // No se actualiza el estado porque falla antes
        }
    }
}