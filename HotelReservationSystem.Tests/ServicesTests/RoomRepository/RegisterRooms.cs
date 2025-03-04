using Moq;
using Moq.EntityFrameworkCore;
using NUnit.Framework;
using HotelReservationSystem.Infrastructure.Data;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationSystem.Tests.RoomRepositoryTests
{
    [TestFixture]
    public class RegisterRooms
    {
        private Mock<HotelDbContext> _contextMock;
        private RoomRepository _roomRepository;
        private Mock<DbSet<Room>> _roomDbSetMock;
        private List<Room> _rooms;

        [SetUp]
        public void Setup()
        {
            _rooms = new List<Room>();
            var options = new DbContextOptions<HotelDbContext>();
            _contextMock = new Mock<HotelDbContext>(options);
            _roomDbSetMock = new Mock<DbSet<Room>>();
            _contextMock.Setup(c => c.Rooms).ReturnsDbSet(_rooms);
            _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            _roomRepository = new RoomRepository(_contextMock.Object);
        }

        /// <summary>
        /// TC-ROOM-001: Verifies that a valid room is added successfully to the database.
        /// </summary>
        [Test]
        public async Task AddAsync_ValidRoom_ShouldAddRoomSuccessfully()
        {
            // Arrange
            var room = new Room { Id = 1, Type = "Deluxe", PricePerNight = 200, Available = true };
            _rooms.Add(room);

            // Act
            var result = await _roomRepository.AddAsync(room);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            _contextMock.Verify(c => c.SaveChangesAsync(default), Times.Once());
        }

        /// <summary>
        /// TC-ROOM-002: Verifies that an exception is thrown when there is an issue adding a room.
        /// </summary>
        [Test]
        public void AddAsync_FailedSave_ShouldThrowException()
        {
            // Arrange
            var room = new Room { Id = 2, Type = "Suite", PricePerNight = 300, Available = true };

            _contextMock.Setup(c => c.SaveChangesAsync(default))
                        .ThrowsAsync(new DbUpdateException("Database error"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<DbUpdateException>(async () =>
                await _roomRepository.AddAsync(room));

            Assert.AreEqual("Database error", ex.Message);
            _contextMock.Verify(c => c.SaveChangesAsync(default), Times.Once());
        }
    }
}
