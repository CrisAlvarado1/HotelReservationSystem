using System.ComponentModel.DataAnnotations;
using HotelReservationSystem.Core.Services;
using HotelReservationSystem.Infrastructure.Interfaces;
using HotelReservationSystem.Infrastructure.Models;
using Moq;

namespace HotelReservationSystem.Tests;

[TestFixture]
public class RegisterRooms
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

    /// <summary>
    /// TC-RH-003 - Test to verify that an ArgumentException is thrown when the price is zero or negative.
    /// </summary>
    [Test]
    [TestCase(0.00)]
    [TestCase(-10.00)]
    public void RegisterRoom_InvalidPrice_ShouldThrowArgumentException(decimal invalidPrice)
    {
        // Arrange
        var invalidPriceRoom = new Room
        {
            Id = 2,
            Type = "Standard",
            PricePerNight = invalidPrice,
            Available = true
        };

        // Act
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _roomService.RegisterRoomAsync(invalidPriceRoom));

        // Assert
        Assert.AreEqual("The price per night must be greater than zero.", ex.Message);
    }

    /// <summary>
    /// TC-RH-004 - Test to verify that a ValidationException is thrown when the room type is empty or null.
    /// </summary>
    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void RegisterRoom_EmptyRoomType_ShouldThrowValidationException(string invalidType)
    {
        // Arrange
        var emptyTypeRoom = new Room
        {
            Id = 3,
            Type = invalidType,
            PricePerNight = 100.00m,
            Available = true
        };

        // Act
        var ex = Assert.ThrowsAsync<ValidationException>(async () =>
            await _roomService.RegisterRoomAsync(emptyTypeRoom));

        // Assert
        Assert.AreEqual("The room type is required.", ex.Message);
    }

    /// <summary>
    /// TC-RH-005 - Test to verify that an ArgumentNullException is thrown when passing a null room.
    /// </summary>
    [Test]
    public void RegisterRoom_NullInput_ShouldThrowArgumentNullException()
    {
        // Act
        Room nullRoom = null;

        // Act
        var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _roomService.RegisterRoomAsync(nullRoom));

        // Assert
        StringAssert.StartsWith("The room cannot be null.", ex.Message);
        Assert.AreEqual("room", ex.ParamName);
    }
}
