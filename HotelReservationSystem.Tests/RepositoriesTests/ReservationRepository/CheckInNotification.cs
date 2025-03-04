using Moq;
using Moq.EntityFrameworkCore;
using NUnit.Framework;
using HotelReservationSystem.Infrastructure.Data;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Infrastructure.Repositories;
using HotelReservationSystem.Infrastructure.Data.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationSystem.Tests.RepositoriesTests
{
    [TestFixture]
    public class CheckInNotification
    {
        private Mock<HotelDbContext> _contextMock;
        private ReservationRepository _reservationRepository;
        private List<Reservation> _reservations;

        [SetUp]
        public void Setup()
        {
            _reservations = new List<Reservation>();
            var options = new DbContextOptions<HotelDbContext>();
            _contextMock = new Mock<HotelDbContext>(options);
            _contextMock.Setup(c => c.Reservations).ReturnsDbSet(_reservations);

            _reservationRepository = new ReservationRepository(_contextMock.Object);
        }

        /// <summary>
        /// TC-RES-REPO-005: Verifies that existing reservations within a date range are retrieved successfully.
        /// </summary>
        [Test]
        public async Task FindReservationsByStartDateRangeAsync_ExistingReservations_ShouldReturnList()
        {
            // Arrange
            var startRange = DateTime.UtcNow.AddDays(1);
            var endRange = DateTime.UtcNow.AddDays(5);

            var reservation1 = new Reservation
            {
                Id = 1,
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(4),
                Status = ReservationStatus.Confirmed
            };

            var reservation2 = new Reservation
            {
                Id = 2,
                ClientId = 2,
                RoomId = 2,
                StartDate = DateTime.UtcNow.AddDays(3),
                EndDate = DateTime.UtcNow.AddDays(6),
                Status = ReservationStatus.Confirmed
            };

            _reservations.AddRange(new[] { reservation1, reservation2 });

            // Act
            var result = await _reservationRepository.FindReservationsByStartDateRangeAsync(startRange, endRange);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        /// <summary>
        /// TC-RES-REPO-006: Verifies that no reservations are returned when none match the date range.
        /// </summary>
        [Test]
        public async Task FindReservationsByStartDateRangeAsync_NoMatchingReservations_ShouldReturnEmptyList()
        {
            // Arrange
            var startRange = DateTime.UtcNow.AddDays(10);
            var endRange = DateTime.UtcNow.AddDays(15);

            var reservation = new Reservation
            {
                Id = 1,
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(4),
                Status = ReservationStatus.Confirmed
            };
            _reservations.Add(reservation);

            // Act
            var result = await _reservationRepository.FindReservationsByStartDateRangeAsync(startRange, endRange);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
    }
}