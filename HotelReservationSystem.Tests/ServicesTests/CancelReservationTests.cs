using Moq;
using HotelReservationSystem.Core.Services;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Infrastructure.Interfaces;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace HotelReservationSystem.Tests
{
    [TestFixture]
    public class CancelReservationTests
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

        [Test]
        public async Task CancelReservationAsync_ConfirmedReservation_CancelsSuccessfully()
        {
            int reservationId = 1;
            var reservation = new Reservation
            {
                Id = reservationId,
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Status = HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };

            _reservationRepositoryMock.Setup(repo => repo.FindByIdAsync(reservationId))
                                     .ReturnsAsync(reservation);
            _reservationRepositoryMock.Setup(repo => repo.HasConfirmedReservationsAsync(
                reservation.RoomId,
                reservation.StartDate,
                reservation.EndDate,
                reservationId))
                               .ReturnsAsync(false);

            await _reservationService.CancelReservationAsync(reservationId);

            Assert.AreEqual(HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Canceled, reservation.Status);
            _reservationRepositoryMock.Verify(repo => repo.UpdateAsync(reservation), Times.Once());
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(reservation.RoomId, true), Times.Once());
        }

        [Test]
        public async Task CancelReservationAsync_ReservationNotFound_ThrowsException()
        {
            int reservationId = 999;

            _reservationRepositoryMock.Setup(repo => repo.FindByIdAsync(reservationId))
                                     .ReturnsAsync((Reservation)null);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _reservationService.CancelReservationAsync(reservationId));
            Assert.AreEqual("Reservation not found.", ex.Message);

            _reservationRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Reservation>()), Times.Never());
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public async Task CancelReservationAsync_NotConfirmedReservation_ThrowsException()
        {
            int reservationId = 1;
            var reservation = new Reservation
            {
                Id = reservationId,
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Status = HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Canceled
            };

            _reservationRepositoryMock.Setup(repo => repo.FindByIdAsync(reservationId))
                                     .ReturnsAsync(reservation);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _reservationService.CancelReservationAsync(reservationId));
            Assert.AreEqual("Only confirmed reservations can be canceled.", ex.Message);

            _reservationRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Reservation>()), Times.Never());
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public async Task CancelReservationAsync_PastStartDate_ThrowsException()
        {
            int reservationId = 1;
            var reservation = new Reservation
            {
                Id = reservationId,
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now.AddDays(1),
                Status = HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };

            _reservationRepositoryMock.Setup(repo => repo.FindByIdAsync(reservationId))
                                     .ReturnsAsync(reservation);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _reservationService.CancelReservationAsync(reservationId));
            Assert.AreEqual("Cannot cancel a reservation that has already started or passed.", ex.Message);

            _reservationRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Reservation>()), Times.Never());
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public async Task CancelReservationAsync_HasOtherConfirmedReservations_DoesNotSetRoomAvailable()
        {
            int reservationId = 1;
            var reservation = new Reservation
            {
                Id = reservationId,
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Status = HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };

            _reservationRepositoryMock.Setup(repo => repo.FindByIdAsync(reservationId))
                                     .ReturnsAsync(reservation);
            _reservationRepositoryMock.Setup(repo => repo.HasConfirmedReservationsAsync(
                reservation.RoomId,
                reservation.StartDate,
                reservation.EndDate,
                reservationId))
                               .ReturnsAsync(true);

            await _reservationService.CancelReservationAsync(reservationId);

            Assert.AreEqual(HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Canceled, reservation.Status);
            _reservationRepositoryMock.Verify(repo => repo.UpdateAsync(reservation), Times.Once());
            _roomRepositoryMock.Verify(repo => repo.UpdateAvailabilityAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never());
        }
    }
}