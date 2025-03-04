using HotelReservationSystem.Infrastructure.Data;
using HotelReservationSystem.Infrastructure.Interfaces;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace HotelReservationSystem.Tests;

[TestFixture]
public class RoomRepositoryTests
{
    private Mock<HotelDbContext> _contextMock;
    private RoomRepository _roomRepository;
    private List<Room> _rooms;
    private List<Reservation> _reservations;

    [SetUp]
    public void Setup()
    {
        _rooms = new List<Room>
            {
                new Room { Id = 1, Type = "Single", PricePerNight = 50, Available = true },
                new Room { Id = 2, Type = "Double", PricePerNight = 80, Available = true },
                new Room { Id = 3, Type = "Suite", PricePerNight = 150, Available = true }
            };

        _reservations = new List<Reservation>
            {
                new Reservation { Id = 1, RoomId = 2, StartDate = DateTime.Now.AddDays(3), EndDate = DateTime.Now.AddDays(5) }
            };

        var options = new DbContextOptions<HotelDbContext>();

        _contextMock = new Mock<HotelDbContext>(options);
        _contextMock.Setup(c => c.Rooms).ReturnsDbSet(_rooms);
        _contextMock.Setup(c => c.Reservations).ReturnsDbSet(_reservations);
        _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        _roomRepository = new RoomRepository(_contextMock.Object);
    }

    /// <summary>
    /// TC-CA-REPO-001 - Test to check if the GetAvailableRoomsAsync method returns the available rooms.
    /// </summary>
    [Test]
    public async Task GetAvailableRoomsAsync_RoomsAreFree_ShouldReturnsAvailableRoom()
    {
        // Arrange
        DateTime startDate = DateTime.Now.AddDays(1);
        DateTime endDate = DateTime.Now.AddDays(4); // Overlaps with Room 2 reservation

        // Act
        var availableRooms = await _roomRepository.GetAvailableRoomsAsync(startDate, endDate);

        // Assert
        Assert.IsNotNull(availableRooms, "The available rooms list should not be null.");
        Assert.That(availableRooms.Count(), Is.EqualTo(2), "Only two rooms should be available.");
        Assert.IsTrue(availableRooms.Any(r => r.Id == 1), "Room 1 should be available.");
        Assert.IsTrue(availableRooms.Any(r => r.Id == 3), "Room 3 should be available.");
        Assert.IsFalse(availableRooms.Any(r => r.Id == 2), "Room 2 should not be available as it is reserved.");
    }

    /// <summary>
    /// TC-CA-REPO-001 - Test to check if the GetAvailableRoomsAsync method returns an empty list when all rooms are reserved.
    /// </summary>
    [Test]
    public async Task GetAvailableRoomsAsync_AllRoomsReserved_ShouldReturnsEmptyList()
    {
        // Arrange
        DateTime startDate = DateTime.Now.AddDays(1);
        DateTime endDate = DateTime.Now.AddDays(4);

        _reservations = new List<Reservation>
            {
                new Reservation { Id = 1, RoomId = 1, StartDate = startDate, EndDate = endDate },
                new Reservation { Id = 2, RoomId = 2, StartDate = startDate, EndDate = endDate },
                new Reservation { Id = 3, RoomId = 3, StartDate = startDate, EndDate = endDate }
            };

        _contextMock.Setup(c => c.Reservations).ReturnsDbSet(_reservations);

        // Act
        var availableRooms = await _roomRepository.GetAvailableRoomsAsync(startDate, endDate);

        // Assert
        Assert.IsNotNull(availableRooms, "The result should not be null.");
        Assert.IsEmpty(availableRooms, "The result should be an empty list because all rooms are reserved.");
    }
}
