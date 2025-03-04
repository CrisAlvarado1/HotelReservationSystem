using Moq;
using Moq.EntityFrameworkCore;
using NUnit.Framework;
using HotelReservationSystem.Infrastructure.Data;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Infrastructure.Repositories;
using HotelReservationSystem.Infrastructure.Data.Enum;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationSystem.Tests.RepositoriesTests
{
    [TestFixture]
    public class CancelReservation
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
        /// TC-RES-REPO-003: Verifies that an existing reservation can be canceled successfully.
        /// </summary>
        [Test]
        public async Task UpdateAsync_ExistingReservation_CancelsSuccessfully()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = 1,
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(3),
                Status = ReservationStatus.Confirmed
            };
            _reservations.Add(reservation);

            // Act
            reservation.Status = ReservationStatus.Canceled;
            await _reservationRepository.UpdateAsync(reservation);

            // Assert
            Assert.AreEqual(ReservationStatus.Canceled, reservation.Status);
            _contextMock.Verify(repo => repo.SaveChangesAsync(default), Times.Once());
        }

        /// <summary>
        /// TC-RES-REPO-004: Verifies that attempting to cancel a non-existing reservation throws an exception.
        /// </summary>
        [Test]
        public async Task UpdateAsync_NonExistingReservation_ThrowsException()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = 999,
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(3),
                Status = ReservationStatus.Canceled
            };

            _contextMock.Setup(c => c.SaveChangesAsync(default))
                        .ThrowsAsync(new DbUpdateConcurrencyException("Reservation not found"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
                await _reservationRepository.UpdateAsync(reservation));

            Assert.AreEqual("Reservation not found", ex.Message);
            _contextMock.Verify(repo => repo.SaveChangesAsync(default), Times.Once());
        }
    }
}