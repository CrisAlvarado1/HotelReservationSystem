using HotelReservationSystem.Core.Services;
using HotelReservationSystem.Infrastructure.Interfaces;
using HotelReservationSystem.Infrastructure.Models;
using Moq;

namespace HotelReservationSystem.Tests;

[TestFixture]
public class SearchRooms
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
    /// TC-SH-006 - Test to verify that the search method returns rooms of a specific type.
    /// </summary>
    [Test]
    public async Task SearchRooms_ByType_ShouldReturnsMatchingRooms()
    {
        // Arrange
        var type = "Double";
        var allRooms = new List<Room>
            {
                new() { Id = 1, Type = "Double", PricePerNight = 150.00m, Available = true },
                new() { Id = 2, Type = "Double", PricePerNight = 150.00m, Available = true },
                new() { Id = 2, Type = "Single", PricePerNight = 120.00m, Available = true }
            };

        _roomRepositoryMock.Setup(repo => repo.SearchAsync(type, null, null, null))
                           .ReturnsAsync(allRooms);

        // Act
        var result = await _roomService.SearchAsync(type, null, null, null);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.All(r => r.Type.Contains("Double")), Is.True);

        _roomRepositoryMock.Verify(repo => repo.SearchAsync(type, null, null, null), Times.Once);
    }

    /// <summary>
    ///  TC-SH-007 - Test to verify that the search method returns rooms within a specific price range.
    /// </summary>
    [Test]
    public async Task SearchRooms_ByPriceRange_ShouldReturnsMatchingRoom()
    {
        // Arrange
        var minPrice = 100.00m;
        var maxPrice = 200.00m;
        var allRooms = new List<Room>
            {
                new() { Id = 1, Type = "Double", PricePerNight = 150.00m, Available = true },
                new() { Id = 2, Type = "Single", PricePerNight = 120.00m, Available = true },
                new() { Id = 3, Type = "Suite", PricePerNight = 80.00m, Available = true },
                new() { Id = 4, Type = "Jr Suite", PricePerNight = 220.00m, Available = true }
            };

        _roomRepositoryMock.Setup(repo => repo.SearchAsync(null, minPrice, maxPrice, null))
                           .ReturnsAsync(allRooms);

        // Act
        var result = await _roomService.SearchAsync(null, minPrice, maxPrice, null);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.All(r => r.PricePerNight >= minPrice && r.PricePerNight <= maxPrice), Is.True,
            "All returned rooms should be within the specified price range.");

        _roomRepositoryMock.Verify(repo => repo.SearchAsync(null, minPrice, maxPrice, null), Times.Once);
    }

    /// <summary>
    /// TC-SH-008 - Test to verify that the search method returns available rooms.
    /// </summary>
    [Test]
    public async Task SearchRooms_ByAvailability_ShouldReturnsAvailableRooms()
    {
        // Arrange
        var available = true;

        var allRooms = new List<Room>
            {
                new() { Id = 1, Type = "Double", PricePerNight = 150.00m, Available = true },
                new() { Id = 2, Type = "Single", PricePerNight = 120.00m, Available = true },
                new() { Id = 3, Type = "Suite", PricePerNight = 220.00m, Available = false }
            };

        _roomRepositoryMock.Setup(repo => repo.SearchAsync(null, null, null, available))
                           .ReturnsAsync(allRooms);

        // Act
        var result = await _roomService.SearchAsync(null, null, null, available);

        // Assert
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.All(r => r.Available == available), Is.True,
            "All returned rooms should be marked as available.");

        _roomRepositoryMock.Verify(repo => repo.SearchAsync(null, null, null, available), Times.Once);
    }

    /// <summary>
    /// TC-SH-009 - Test to verify that an exception is thrown when invalid price range values are provided.
    /// </summary>
    [Test]
    [TestCase(-10.00, 200.00)]
    [TestCase(200.00, 100.00)]
    [TestCase(null, -50.00)]
    public async Task SearchRooms_InvalidPriceRange_ShouldThrowException(decimal? minPrice, decimal? maxPrice)
    {
        // Arrange
        var invalidMinPrice = minPrice;
        var invalidMaxPrice = maxPrice;

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _roomService.SearchAsync(null, invalidMinPrice, invalidMaxPrice, null));

        // Assert
        Assert.That(exception.Message, Is.EqualTo("Invalid price range provided."));
    }

    /// <summary>
    /// TC-SH-010 - Test to verify that an empty list is returned when no rooms match the search criteria.
    /// </summary>
    [Test]
    public async Task SearchRooms_EmptyResult_ShouldReturnsEmptyList()
    {
        // Arrange
        var type = "Triple";

        _roomRepositoryMock.Setup(repo => repo.SearchAsync(type, 100.00m, 110.00m, true))
                           .ReturnsAsync(new List<Room>());

        // Act
        var result = await _roomService.SearchAsync(type, 100.00m, 110.00m, true);

        // Assert
        Assert.That(result, Is.Not.Null, "The result should not be null.");
        Assert.That(result, Is.Empty, "The result should be an empty list.");
    }
}
