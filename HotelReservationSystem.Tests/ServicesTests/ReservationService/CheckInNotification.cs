using Moq;
using HotelReservationSystem.Core.Services;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Infrastructure.Interfaces;

namespace HotelReservationSystem.Tests
{
    [TestFixture]
    public class CheckInNotification
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
        /// TC-CHECKIN-001: Verify that notifications are sent for upcoming reservations within the next 2 days.
        /// </summary>
        [Test]
        public async Task NotifyCheckInAsync_UpcomingReservations_SendsNotifications()
        {
            var currentDate = DateTime.UtcNow.Date;
            var reservation1 = new Reservation
            {
                Id = 1,
                ClientId = 1,
                RoomId = 1,
                StartDate = currentDate.AddDays(1).ToUniversalTime(),
                EndDate = currentDate.AddDays(3).ToUniversalTime(),
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed,
                IsNotified = false 
            };
            var reservation2 = new Reservation
            {
                Id = 2,
                ClientId = 2,
                RoomId = 2,
                StartDate = currentDate.AddDays(2).ToUniversalTime(),
                EndDate = currentDate.AddDays(4).ToUniversalTime(),
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed,
                IsNotified = false 
            };
            var reservationsInRange = new List<Reservation> { reservation1, reservation2 };

            _reservationRepositoryMock.Setup(repo => repo.FindReservationsByStartDateRangeAsync(
                It.Is<DateTime>(d => d == currentDate),
                It.Is<DateTime>(d => d == currentDate.AddDays(2))))
                .ReturnsAsync(reservationsInRange);

            _reservationRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Reservation>())).Returns(Task.CompletedTask);

            var notifications = await _reservationService.NotifyCheckInAsync();

            Assert.AreEqual(2, notifications.Count); 
            Assert.IsTrue(notifications[0].Contains($"Dear Client {reservation1.ClientId}, your reservation (ID: {reservation1.Id}) check-in is on {reservation1.StartDate.ToString("dd/MM/yyyy")}"));
            Assert.IsTrue(notifications[1].Contains($"Dear Client {reservation2.ClientId}, your reservation (ID: {reservation2.Id}) check-in is on {reservation2.StartDate.ToString("dd/MM/yyyy")}"));
            Assert.IsTrue(reservation1.IsNotified); 
            Assert.IsTrue(reservation2.IsNotified); 
            _reservationRepositoryMock.Verify(repo => repo.UpdateAsync(reservation1), Times.Once());
            _reservationRepositoryMock.Verify(repo => repo.UpdateAsync(reservation2), Times.Once());
        }

        /// <summary>
        /// TC-CHECKIN-002: Verify that no notifications are sent if there are no upcoming reservations.
        /// </summary>
        [Test]
        public async Task NotifyCheckInAsync_NoUpcomingReservations_ReturnsEmptyList()
        {
            var currentDate = DateTime.UtcNow.Date;
            var reservationsInRange = new List<Reservation>();

            _reservationRepositoryMock.Setup(repo => repo.FindReservationsByStartDateRangeAsync(
                It.Is<DateTime>(d => d == currentDate),
                It.Is<DateTime>(d => d == currentDate.AddDays(2))))
                .ReturnsAsync(reservationsInRange);

            var notifications = await _reservationService.NotifyCheckInAsync();

            Assert.AreEqual(1, notifications.Count); 
            Assert.AreEqual($"No confirmed reservations found in the date range ({currentDate:dd/MM/yyyy} to {currentDate.AddDays(2):dd/MM/yyyy}).", notifications[0]);
            _reservationRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Reservation>()), Times.Never());
        }

        /// <summary>
        /// TC-CHECKIN-003: Verify that only confirmed reservations are included in notifications.
        /// </summary>
        [Test]
        public async Task NotifyCheckInAsync_NonConfirmedReservations_ExcludesFromNotifications()
        {
            var currentDate = DateTime.UtcNow.Date; 
            var confirmedReservation = new Reservation
            {
                Id = 1,
                ClientId = 1,
                RoomId = 1,
                StartDate = currentDate.AddDays(1).ToUniversalTime(),
                EndDate = currentDate.AddDays(3).ToUniversalTime(),
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed,
                IsNotified = false
            };
            var canceledReservation = new Reservation
            {
                Id = 2,
                ClientId = 2,
                RoomId = 2,
                StartDate = currentDate.AddDays(1).ToUniversalTime(), 
                EndDate = currentDate.AddDays(3).ToUniversalTime(),
                Status = Infrastructure.Data.Enum.ReservationStatus.Canceled,
                IsNotified = false
            };
            var reservationsInRange = new List<Reservation> { confirmedReservation, canceledReservation };
            var reservationsToNotify = reservationsInRange.Where(r => r.Status == Infrastructure.Data.Enum.ReservationStatus.Confirmed && !r.IsNotified).ToList();

            _reservationRepositoryMock.Setup(repo => repo.FindReservationsByStartDateRangeAsync(
                It.Is<DateTime>(d => d == currentDate),
                It.Is<DateTime>(d => d == currentDate.AddDays(2))))
                .ReturnsAsync(reservationsInRange.Where(r => r.Status == Infrastructure.Data.Enum.ReservationStatus.Confirmed).ToList());

            _reservationRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Reservation>())).Returns(Task.CompletedTask);

            var notifications = await _reservationService.NotifyCheckInAsync();

            Assert.AreEqual(1, notifications.Count); 
            Assert.IsTrue(notifications[0].Contains($"Dear Client {confirmedReservation.ClientId}, your reservation (ID: {confirmedReservation.Id})")); 
            Assert.IsTrue(confirmedReservation.IsNotified); 
            Assert.IsFalse(canceledReservation.IsNotified);
            _reservationRepositoryMock.Verify(repo => repo.UpdateAsync(confirmedReservation), Times.Once());
            _reservationRepositoryMock.Verify(repo => repo.UpdateAsync(canceledReservation), Times.Never());
        }

        /// <summary>
        /// TC-CHECKIN-004: Verify that reservations already notified are excluded from notifications.
        /// </summary>
        [Test]
        public async Task NotifyCheckInAsync_AlreadyNotifiedReservations_ExcludesFromNotifications()
        {
            var currentDate = DateTime.UtcNow.Date;
            var reservation1 = new Reservation
            {
                Id = 1,
                ClientId = 1,
                RoomId = 1,
                StartDate = currentDate.AddDays(1).ToUniversalTime(),
                EndDate = currentDate.AddDays(3).ToUniversalTime(),
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed,
                IsNotified = false 
            };
            var reservation2 = new Reservation
            {
                Id = 2,
                ClientId = 2,
                RoomId = 2,
                StartDate = currentDate.AddDays(1).ToUniversalTime(),
                EndDate = currentDate.AddDays(3).ToUniversalTime(),
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed,
                IsNotified = true 
            };
            var reservationsInRange = new List<Reservation> { reservation1, reservation2 };
            var reservationsToNotify = reservationsInRange.Where(r => !r.IsNotified).ToList(); 

            _reservationRepositoryMock.Setup(repo => repo.FindReservationsByStartDateRangeAsync(
                It.Is<DateTime>(d => d == currentDate),
                It.Is<DateTime>(d => d == currentDate.AddDays(2))))
                .ReturnsAsync(reservationsInRange);

            _reservationRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Reservation>())).Returns(Task.CompletedTask);

            var notifications = await _reservationService.NotifyCheckInAsync();

            Assert.AreEqual(1, notifications.Count); 
            Assert.IsTrue(notifications[0].Contains($"Dear Client {reservation1.ClientId}, your reservation (ID: {reservation1.Id})")); 
            Assert.IsTrue(reservation1.IsNotified); 
            Assert.IsTrue(reservation2.IsNotified);
            _reservationRepositoryMock.Verify(repo => repo.UpdateAsync(reservation1), Times.Once());
            _reservationRepositoryMock.Verify(repo => repo.UpdateAsync(reservation2), Times.Never());
        }

        /// <summary>
        /// TC-CHECKIN-005: Verify that a message is returned when all reservations in the range are already notified.
        /// </summary>
        [Test]
        public async Task NotifyCheckInAsync_AllReservationsNotified_ReturnsMessageWithIds()
        {
            var currentDate = DateTime.UtcNow.Date;
            var reservation1 = new Reservation
            {
                Id = 1,
                ClientId = 1,
                RoomId = 1,
                StartDate = currentDate.AddDays(1).ToUniversalTime(), 
                EndDate = currentDate.AddDays(3).ToUniversalTime(),
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed,
                IsNotified = true
            };
            var reservation2 = new Reservation
            {
                Id = 2,
                ClientId = 2,
                RoomId = 2,
                StartDate = currentDate.AddDays(1).ToUniversalTime(),
                EndDate = currentDate.AddDays(3).ToUniversalTime(),
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed,
                IsNotified = true
            };
            var reservationsInRange = new List<Reservation> { reservation1, reservation2 };

            _reservationRepositoryMock.Setup(repo => repo.FindReservationsByStartDateRangeAsync(
                It.Is<DateTime>(d => d == currentDate),
                It.Is<DateTime>(d => d == currentDate.AddDays(2))))
                .ReturnsAsync(reservationsInRange);

            var notifications = await _reservationService.NotifyCheckInAsync();

            Assert.AreEqual(1, notifications.Count); 
            Assert.IsTrue(notifications[0].Contains($"All reservations in the date range ({currentDate:dd/MM/yyyy} to {currentDate.AddDays(2):dd/MM/yyyy}) have already been notified"));
            Assert.IsTrue(notifications[0].Contains("Reservation IDs: 1, 2")); 
            _reservationRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Reservation>()), Times.Never());
        }
    }
}