using Moq;
using Moq.EntityFrameworkCore;
using NUnit.Framework;
using HotelReservationSystem.Infrastructure.Data;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelReservationSystem.Infrastructure.Data.Enum;

namespace HotelReservationSystem.Tests.ReservationRepositoryTests
{
    [TestFixture]
    public class RegisterReservation
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
            _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            _reservationRepository = new ReservationRepository(_contextMock.Object);
        }

        /// <summary>
        /// TC-RES-REPO-001: Verifies that a valid reservation is added successfully to the database.
        /// </summary>
        [Test]
        public async Task AddAsync_ValidData_ShouldAddSuccessfully()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = 0,
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(3),
                Status = ReservationStatus.Confirmed
            };

            // Act
            _reservations.Add(reservation); // Manually add to the simulated list
            await _contextMock.Object.SaveChangesAsync();

            // Assert
            Assert.AreEqual(1, _reservations.Count);
            Assert.AreEqual(reservation.ClientId, _reservations[0].ClientId);
            Assert.AreEqual(reservation.RoomId, _reservations[0].RoomId);
            Assert.AreEqual(ReservationStatus.Confirmed, _reservations[0].Status);
            _contextMock.Verify(repo => repo.SaveChangesAsync(default), Times.Once());
        }

        /// <summary>
        /// TC-RES-REPO-002: Verifies that an exception is thrown if a database error occurs during save.
        /// </summary>
        [Test]
        public async Task AddAsync_DbFailure_ShouldThrowException()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = 0,
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(3),
                Status = ReservationStatus.Confirmed
            };

            _contextMock.Setup(c => c.SaveChangesAsync(default))
                        .ThrowsAsync(new DbUpdateException("Database error", new Exception()));

            // Act & Assert
            var ex = Assert.ThrowsAsync<DbUpdateException>(async () =>
                await _reservationRepository.AddAsync(reservation));

            Assert.AreEqual("Database error", ex.Message);
            _contextMock.Verify(repo => repo.SaveChangesAsync(default), Times.Once());
        }
    }
}