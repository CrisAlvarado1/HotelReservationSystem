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
            // Arrange: Set up the test data with upcoming reservations
            var currentDate = DateTime.UtcNow.Date; // Use UTC for consistency
            var reservation1 = new Reservation
            {
                Id = 1,
                ClientId = 1,
                RoomId = 1,
                StartDate = currentDate.AddDays(1).ToUniversalTime(), // Tomorrow in UTC
                EndDate = currentDate.AddDays(3).ToUniversalTime(),
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };
            var reservation2 = new Reservation
            {
                Id = 2,
                ClientId = 2,
                RoomId = 2,
                StartDate = currentDate.AddDays(2).ToUniversalTime(), // In 2 days in UTC
                EndDate = currentDate.AddDays(4).ToUniversalTime(),
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };
            var upcomingReservations = new List<Reservation> { reservation1, reservation2 };

            // Mock the repository to return upcoming reservations
            _reservationRepositoryMock.Setup(repo => repo.FindReservationsByStartDateRangeAsync(
                It.Is<DateTime>(d => d == currentDate),
                It.Is<DateTime>(d => d == currentDate.AddDays(2))))
                .ReturnsAsync(upcomingReservations);

            // Act: Call the method to send notifications
            var notifications = await _reservationService.NotifyCheckInAsync();

            Assert.AreEqual(2, notifications.Count); // Two reservations should receive notifications
            Assert.IsTrue(notifications[0].Contains($"reservation (ID: {reservation1.Id}) check-in is on {reservation1.StartDate.ToString("dd/MM/yyyy")}"));
            Assert.IsTrue(notifications[1].Contains($"reservation (ID: {reservation2.Id}) check-in is on {reservation2.StartDate.ToString("dd/MM/yyyy")}"));
        }

        /// <summary>
        /// TC-CHECKIN-002: Verify that no notifications are sent if there are no upcoming reservations.
        /// </summary>
        [Test]
        public async Task NotifyCheckInAsync_NoUpcomingReservations_ReturnsEmptyList()
        {
            var currentDate = DateTime.UtcNow.Date; // Use UTC
            var upcomingReservations = new List<Reservation>();

            _reservationRepositoryMock.Setup(repo => repo.FindReservationsByStartDateRangeAsync(
                It.Is<DateTime>(d => d == currentDate),
                It.Is<DateTime>(d => d == currentDate.AddDays(2))))
                .ReturnsAsync(upcomingReservations);

            // Act: Call the method to send notifications
            var notifications = await _reservationService.NotifyCheckInAsync();

            // Assert: Verify the results
            Assert.IsEmpty(notifications); // No notifications should be sent
        }

        /// <summary>
        /// TC-CHECKIN-003: Verify that only confirmed reservations are included in notifications.
        /// </summary>
        [Test]
        public async Task NotifyCheckInAsync_NonConfirmedReservations_ExcludesFromNotifications()
        {
            // Arrange: Set up the test with a mix of confirmed and canceled reservations
            var currentDate = DateTime.UtcNow.Date; // Use UTC
            var confirmedReservation = new Reservation
            {
                Id = 1,
                ClientId = 1,
                RoomId = 1,
                StartDate = currentDate.AddDays(1).ToUniversalTime(), // Tomorrow in UTC
                EndDate = currentDate.AddDays(3).ToUniversalTime(),
                Status = Infrastructure.Data.Enum.ReservationStatus.Confirmed
            };
            var canceledReservation = new Reservation
            {
                Id = 2,
                ClientId = 2,
                RoomId = 2,
                StartDate = currentDate.AddDays(1).ToUniversalTime(), // Tomorrow in UTC
                EndDate = currentDate.AddDays(3).ToUniversalTime(),
                Status = Infrastructure.Data.Enum.ReservationStatus.Canceled
            };
            var reservations = new List<Reservation> { confirmedReservation, canceledReservation };

            // Mock the repository to return both reservations
            // Note: The repository method should filter out non-confirmed reservations
            _reservationRepositoryMock.Setup(repo => repo.FindReservationsByStartDateRangeAsync(
                It.Is<DateTime>(d => d == currentDate),
                It.Is<DateTime>(d => d == currentDate.AddDays(2))))
                .ReturnsAsync(new List<Reservation> { confirmedReservation }); // Only confirmed reservations

            // Act: Call the method to send notifications
            var notifications = await _reservationService.NotifyCheckInAsync();

            // Assert: Verify the results
            Assert.AreEqual(1, notifications.Count); // Only one notification should be sent (for the confirmed reservation)
            Assert.IsTrue(notifications[0].Contains($"reservation (ID: {confirmedReservation.Id})")); // Only confirmed reservation is included
        }
    }
}