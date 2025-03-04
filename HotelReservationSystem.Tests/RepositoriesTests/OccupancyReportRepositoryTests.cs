using HotelReservationSystem.Infrastructure.Data;
using HotelReservationSystem.Infrastructure.Interfaces;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace HotelReservationSystem.Tests;

[TestFixture]
public class OccupancyReportRepositoryTests
{
    private Mock<HotelDbContext> _contextMock;
    private OccupancyReportRepository _occupancyReportRepository;
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
            new Reservation { Id = 1, RoomId = 1, StartDate = DateTime.Now.AddDays(2), EndDate = DateTime.Now.AddDays(4) },
            new Reservation { Id = 2, RoomId = 2, StartDate = DateTime.Now.AddDays(3), EndDate = DateTime.Now.AddDays(6) },
            new Reservation { Id = 3, RoomId = 1, StartDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(7) }
        };

        var options = new DbContextOptions<HotelDbContext>();

        _contextMock = new Mock<HotelDbContext>(options);
        _contextMock.Setup(c => c.Rooms).ReturnsDbSet(_rooms);
        _contextMock.Setup(c => c.Reservations).ReturnsDbSet(_reservations);
        _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        _occupancyReportRepository = new OccupancyReportRepository(_contextMock.Object);
    }


    /// <summary>
    /// TC-OR-REPO-001 - Test to check if the GetOccupancyRateAsync method returns the correct occupancy rates.
    /// </summary>
    [Test]
    public async Task GetOccupancyRateAsync_ValidDateRange_ShouldReturnCorrectRates()
    {
        // Arrange
        DateTime startDate = DateTime.Now.AddDays(1);
        DateTime endDate = DateTime.Now.AddDays(7);

        _reservations = new List<Reservation>
            {
                new Reservation { Id = 1, RoomId = 1, StartDate = startDate, EndDate = endDate },
                new Reservation { Id = 2, RoomId = 2, StartDate = startDate.AddDays(1), EndDate = endDate },
                new Reservation { Id = 3, RoomId = 1, StartDate = startDate.AddDays(2), EndDate = endDate }
            };

        _contextMock.Setup(c => c.Reservations).ReturnsDbSet(_reservations);
        _contextMock.Setup(c => c.Rooms).ReturnsDbSet(_rooms);

        // Mock for GetTotalRoomsByTypeAsync: Returns the total rooms by type
        var totalRoomsByType = _rooms.GroupBy(r => r.Type).ToDictionary(g => g.Key, g => g.Count());

        var repositoryMock = new Mock<IOccupancyReportRepository>();
        repositoryMock
            .Setup(repo => repo.GetTotalRoomsByTypeAsync(It.IsAny<string>()))
            .ReturnsAsync((string roomType) => totalRoomsByType.ContainsKey(roomType) ? totalRoomsByType[roomType] : 0);

        // Act
        var occupancyRates = await _occupancyReportRepository.GetOccupancyRateAsync(startDate, endDate);

        // Assert
        Assert.IsNotNull(occupancyRates, "The result should not be null.");
        Assert.That(occupancyRates.ContainsKey("Single"), "The report should contain occupancy for Single rooms.");
        Assert.That(occupancyRates.ContainsKey("Double"), "The report should contain occupancy for Double rooms.");

        double expectedSingleOccupancy = (double)2 / totalRoomsByType["Single"] * 100;
        double expectedDoubleOccupancy = (double)1 / totalRoomsByType["Double"] * 100;

        Assert.AreEqual(Math.Round(expectedSingleOccupancy, 2), occupancyRates["Single"], "Incorrect occupancy rate for Single rooms.");
        Assert.AreEqual(Math.Round(expectedDoubleOccupancy, 2), occupancyRates["Double"], "Incorrect occupancy rate for Double rooms.");
    }

    /// <summary>
    /// TC-OR-REPO-002 - Test to check if the GetOccupancyRateAsync method throws an exception when no rooms are registered.
    /// </summary>
    [Test]
    public void GetOccupancyRateAsync_NoRoomsRegistered_ShouldThrowException()
    {
        // Arrange
        DateTime startDate = DateTime.Now.AddDays(1);
        DateTime endDate = DateTime.Now.AddDays(7);

        _rooms.Clear(); // Ensure there are no rooms in the system
        _contextMock.Setup(c => c.Rooms).ReturnsDbSet(_rooms);

        _reservations = new List<Reservation>
            {
                new Reservation { Id = 1, RoomId = 1, StartDate = startDate, EndDate = endDate },
                new Reservation { Id = 2, RoomId = 2, StartDate = startDate.AddDays(1), EndDate = endDate }
            };

        _contextMock.Setup(c => c.Reservations).ReturnsDbSet(_reservations);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _occupancyReportRepository.GetOccupancyRateAsync(startDate, endDate));

        Assert.That(exception.Message, Is.EqualTo("No rooms found in the system."));
    }

    /// <summary>
    /// TC-OR-REPO-003 - Test to check if the GetTotalRoomsByTypeAsync method returns the correct number of rooms by type.
    /// </summary>
    [Test]
    public async Task GetTotalRoomsByTypeAsync_ValidRoomType_ShouldReturnCorrectCount()
    {
        // Arrange
        string validRoomType = "Double"; // A room type that exists in the database

        _rooms = new List<Room>
    {
        new Room { Id = 1, Type = "Single", PricePerNight = 50, Available = true },
        new Room { Id = 2, Type = "Double", PricePerNight = 80, Available = true },
        new Room { Id = 3, Type = "Double", PricePerNight = 80, Available = true },
        new Room { Id = 4, Type = "Suite", PricePerNight = 150, Available = true }
    };

        _contextMock.Setup(c => c.Rooms).ReturnsDbSet(_rooms);

        int expectedCount = _rooms.Count(r => r.Type == validRoomType);

        // Act
        int roomCount = await _occupancyReportRepository.GetTotalRoomsByTypeAsync(validRoomType);

        // Assert
        Assert.AreEqual(expectedCount, roomCount, $"The method should return the correct count of rooms for type '{validRoomType}'.");
    }

    /// <summary>
    /// TC-OR-REPO-004 - Test to check if the GetTotalRoomsByTypeAsync method returns the correct number of rooms by type.
    /// </summary>
    [Test]
    public async Task GetTotalRoomsByTypeAsync_NonExistingRoomType_ShouldReturnZero()
    {
        // Arrange
        string nonExistingRoomType = "Penthouse"; // Room type that does not exist in the database

        _contextMock.Setup(c => c.Rooms).ReturnsDbSet(_rooms); // Ensure the mock is properly set up

        // Act
        int roomCount = await _occupancyReportRepository.GetTotalRoomsByTypeAsync(nonExistingRoomType);

        // Assert
        Assert.AreEqual(0, roomCount, "The method should return 0 when the requested room type does not exist.");
    }
}
