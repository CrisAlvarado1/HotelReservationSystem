using Moq;
using Moq.EntityFrameworkCore;
using NUnit.Framework;
using HotelReservationSystem.Infrastructure.Data;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelReservationSystem.Infrastructure.Data.Enum;

namespace HotelReservationSystem.Tests.RepositoriesTests
{
    [TestFixture]
    public class ReservationHistory
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
        /// TC-RES-REPO-001: Verifies that a user's booking history is returned when the user ID is valid.
        /// </summary>
        [Test]
        public async Task GetUserReservationHistoryAsync_ValidUserId_ShouldReturnReservations()
        {
            // Arrange
            var userId = 1;
            var room = new Room { Id = 1, Type = "Standard", PricePerNight = 100.0m, Available = true };
            var reservations = new List<Reservation>
            {
                new Reservation
                {
                    Id = 1,
                    ClientId = userId,
                    RoomId = room.Id,
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(3),
                    Status = ReservationStatus.Confirmed,
                    Room = room
                },
                new Reservation
                {
                    Id = 2,
                    ClientId = userId,
                    RoomId = room.Id,
                    StartDate = DateTime.UtcNow.AddDays(5),
                    EndDate = DateTime.UtcNow.AddDays(7),
                    Status = ReservationStatus.Confirmed,
                    Room = room
                }
            };

            _reservations.AddRange(reservations);


            var result = await _reservationRepository.GetUserReservationHistoryAsync(userId);


            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(reservations[1].Id, result.First().Id);
            Assert.AreEqual(reservations[0].Id, result.Last().Id);
        }

        /// <summary>
        /// TC-RES-REPO-002: Verifies that an empty list is returned when there are no reservations associated with the user ID.
        /// </summary>
        [Test]
        public async Task GetUserReservationHistoryAsync_InvalidUserId_ShouldReturnEmptyList()
        {

            var userId = 999;

            _contextMock.Setup(c => c.Reservations).ReturnsDbSet(new List<Reservation>());

            var result = await _reservationRepository.GetUserReservationHistoryAsync(userId);

            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }
    }
}