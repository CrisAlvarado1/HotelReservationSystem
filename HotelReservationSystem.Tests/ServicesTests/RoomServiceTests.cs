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
}
