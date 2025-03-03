using HotelReservationSystem.Core.Services;
using HotelReservationSystem.Infrastructure.Interfaces;
using HotelReservationSystem.Infrastructure.Models;
using Moq;

namespace HotelReservationSystem.Tests;

public class CheckAvailability
{

    private Mock<IRoomRepository> _roomRepositoryMock;
    private RoomService _roomService;

    [SetUp]
    public void Setup()
    {
        _roomRepositoryMock = new Mock<IRoomRepository>();
        _roomService = new RoomService(_roomRepositoryMock.Object);
    }

    /// <summary>
    /// TC-CA-001 - Test to verify that the check availability method returns available rooms.
    /// </summary>
    [Test]
    public async Task CheckAvailability_DatesAreValid_ShouldReturnsAvailableRooms()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(1);
        var endDate = DateTime.Now.AddDays(3);

        var expectedRooms = new List<Room>
        {
            new() { Id = 1, Type = "Double", PricePerNight = 150.00m, Available = true },
            new() { Id = 2, Type = "Double", PricePerNight = 150.00m, Available = true },
            new() { Id = 3, Type = "Single", PricePerNight = 120.00m, Available = true }
        };

        _roomRepositoryMock.Setup(repo => repo.GetAvailableRoomsAsync(startDate, endDate))
                       .ReturnsAsync(expectedRooms);

        // Act
        var result = await _roomService.CheckAvailabilityAsync(startDate, endDate);

        // Assert
        Assert.That(result, Is.Not.Null, "The result should not be null.");
        Assert.That(result.Count(), Is.EqualTo(expectedRooms.Count), "The number of returned rooms should match the expected count.");
        Assert.That(result.All(r => r.Available), Is.True, "All returned rooms should be available.");
        _roomRepositoryMock.Verify(repo => repo.GetAvailableRoomsAsync(startDate, endDate), Times.Once);
    }

    /// <summary>
    /// TC-CA-002 - Test to verify that the check availability method returns an empty list when no rooms are available.
    /// </summary>
    [Test]
    public async Task CheckAvailability_NoRoomsAvailable_ShouldReturnsEmptyList()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(1);
        var endDate = DateTime.Now.AddDays(3);
        _roomRepositoryMock.Setup(repo => repo.GetAvailableRoomsAsync(startDate, endDate))
                           .ReturnsAsync(new List<Room>());

        // Act
        var result = await _roomService.CheckAvailabilityAsync(startDate, endDate);

        // Assert
        Assert.That(result, Is.Not.Null, "The result should not be null.");
        Assert.That(result, Is.Empty, "The result should be an empty list.");
        _roomRepositoryMock.Verify(repo => repo.GetAvailableRoomsAsync(startDate, endDate), Times.Once);
    }

    /// <summary>
    /// TC-CA-003 - Test to verify that an exception is thrown when startDate is after endDate.
    /// </summary>
    [Test]
    public async Task CheckAvailability_StartDateIsAfterEndDate_ShouldThrowsException()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(5);
        var endDate = DateTime.Now.AddDays(3);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _roomService.CheckAvailabilityAsync(startDate, endDate));

        // Assert
        Assert.That(exception.Message, Is.EqualTo("Start date must be before end date."));
    }
}
