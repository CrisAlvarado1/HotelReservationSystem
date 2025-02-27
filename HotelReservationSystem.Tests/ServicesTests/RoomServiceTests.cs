using Moq;
using HotelReservationSystem.Core.Services;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Infrastructure.Interfaces;

namespace HotelReservationSystem.Tests;

[TestFixture]
public class RoomServiceTests
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
    /// TC-RH-001 - Test to verify that a room is registered successfully.
    /// </summary>
    [Test]
    public async Task RegisterRoom_ValidData_ShouldRegisterSuccessfully()
    {
        // Arrange
        var room = new Room
        {
            Id = 1,
            Type = "Double",
            PricePerNight = 150.00m,
            Available = true
        };

        _roomRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Room>()))
                           .ReturnsAsync(room);

        // Act
        var result = await _roomService.RegisterRoomAsync(room);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(room.Type, result.Type);
        Assert.AreEqual(room.PricePerNight, result.PricePerNight);
        Assert.IsTrue(result.Available);

        _roomRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Room>(r => r.Type == "Double")), Times.Once);
    }

    /// <summary>
    /// TC-RH-002 - Test to verify that an exception is thrown when the room is null.
    /// </summary>
    [Test]
    public void RegisterRoom_DuplicateRoomNumber_ShouldThrowException()
    {
        // Arrange
        var duplicateRoom = new Room
        {
            Id = 1, // Duplicate ID
            Type = "Double",
            PricePerNight = 150.00m,
            Available = true
        };

        _roomRepositoryMock.Setup(repo => repo.FindByIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(new Room { Id = 1, Type = "Standard", PricePerNight = 120.00m, Available = true });

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _roomService.RegisterRoomAsync(duplicateRoom));

        Assert.AreEqual("A room with the same number already exists.", ex.Message);

        _roomRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Room>()), Times.Never);
        _roomRepositoryMock.Verify(repo => repo.FindByIdAsync(It.IsAny<int>()), Times.Once);
    }
}
