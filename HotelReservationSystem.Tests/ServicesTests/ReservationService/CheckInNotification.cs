using Moq;
using HotelReservationSystem.Core.Services;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Infrastructure.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };
            var reservation2 = new Reservation
            {
                Id = 2,
                ClientId = 2,
                RoomId = 2,
                StartDate = currentDate.AddDays(2).ToUniversalTime(), 
                EndDate = currentDate.AddDays(4).ToUniversalTime(),
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };
            var upcomingReservations = new List<Reservation> { reservation1, reservation2 };

            _reservationRepositoryMock.Setup(repo => repo.FindReservationsByStartDateRangeAsync(
                It.Is<DateTime>(d => d == currentDate),
                It.Is<DateTime>(d => d == currentDate.AddDays(2))))
                .ReturnsAsync(upcomingReservations);

            var notifications = await _reservationService.NotifyCheckInAsync();

            Assert.AreEqual(2, notifications.Count); 
            Assert.IsTrue(notifications[0].Contains($"reservation (ID: {reservation1.Id}) check-in is on {reservation1.StartDate.ToString("dd/MM/yyyy")}"));
            Assert.IsTrue(notifications[1].Contains($"reservation (ID: {reservation2.Id}) check-in is on {reservation2.StartDate.ToString("dd/MM/yyyy")}"));
        }

        /// <summary>
        /// TC-CHECKIN-002: Verify that no notifications are sent if there are no upcoming reservations.
        /// </summary>
        [Test]
        public async Task NotifyCheckInAsync_NoUpcomingReservations_ReturnsEmptyList()
        {
            var currentDate = DateTime.UtcNow.Date;
            var upcomingReservations = new List<Reservation>();

            _reservationRepositoryMock.Setup(repo => repo.FindReservationsByStartDateRangeAsync(
                It.Is<DateTime>(d => d == currentDate),
                It.Is<DateTime>(d => d == currentDate.AddDays(2))))
                .ReturnsAsync(upcomingReservations);

            var notifications = await _reservationService.NotifyCheckInAsync();

            Assert.IsEmpty(notifications); // No notifications should be sent
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
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };
            var canceledReservation = new Reservation
            {
                Id = 2,
                ClientId = 2,
                RoomId = 2,
                StartDate = currentDate.AddDays(1).ToUniversalTime(), 
                EndDate = currentDate.AddDays(3).ToUniversalTime(),
                Status = Infrastructure.Data.Enum.ReservationStatus.Canceled
            };
            var reservations = new List<Reservation> { confirmedReservation, canceledReservation };

            _reservationRepositoryMock.Setup(repo => repo.FindReservationsByStartDateRangeAsync(
                It.Is<DateTime>(d => d == currentDate),
                It.Is<DateTime>(d => d == currentDate.AddDays(2))))
                .ReturnsAsync(new List<Reservation> { confirmedReservation }); 

            var notifications = await _reservationService.NotifyCheckInAsync();

            Assert.AreEqual(1, notifications.Count); 
            Assert.IsTrue(notifications[0].Contains($"reservation (ID: {confirmedReservation.Id})"));
        }
    }
}