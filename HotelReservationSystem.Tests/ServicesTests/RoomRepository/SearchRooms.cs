using Moq;
using NUnit.Framework;
using HotelReservationSystem.Infrastructure.Data;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservationSystem.Tests.RoomRepositoryTests
{
    [TestFixture]
    public class SearchRoomsTests
    {
        private Mock<HotelDbContext> _contextMock;
        private RoomRepository _roomRepository;
        private List<Room> _rooms;

        [SetUp]
        public void Setup()
        {
            _rooms = new List<Room>
            {
                new Room { Id = 1, Type = "Deluxe", PricePerNight = 200, Available = true },
                new Room { Id = 2, Type = "Suite", PricePerNight = 300, Available = false },
                new Room { Id = 3, Type = "Deluxe", PricePerNight = 150, Available = true }
            };

            var options = new DbContextOptions<HotelDbContext>();
            _contextMock = new Mock<HotelDbContext>(options);

            var mockSet = new Mock<DbSet<Room>>();
            mockSet.Setup(m => m.AsQueryable()).Returns(_rooms.AsQueryable());

            _contextMock.Setup(c => c.Rooms).Returns(mockSet.Object);

            _roomRepository = new RoomRepository(_contextMock.Object);
        }

        /// <summary>
        /// TC-ROOM-009: Verifies that rooms matching the search criteria are returned successfully.
        /// </summary>
        [Test]
        public async Task SearchAsync_ValidCriteria_ReturnsMatchingRooms()
        {
            string? type = "Deluxe";      
            decimal? minPrice = null;       
            decimal? maxPrice = null;      
            bool? available = true;    

            var result = await _roomRepository.SearchAsync(type, minPrice, maxPrice, available);

            Console.WriteLine($"Result count: {result.Count()}");
            foreach (var room in result)
            {
                Console.WriteLine($"Room: Id={room.Id}, Type={room.Type}, Available={room.Available}");
            }

            // Assert: Verify the expected outcomes
            Assert.IsNotNull(result, "The result should not be null"); 
            Assert.AreEqual(2, result.Count(), "Should return 2 rooms"); 
            Assert.IsTrue(result.All(r => r.Type.Contains("Deluxe") && r.Available), "All rooms should be Deluxe and available"); // Validate all returned rooms match criteria
            Assert.IsTrue(result.Any(r => r.Id == 1), "Should include the room with Id 1"); 
            Assert.IsTrue(result.Any(r => r.Id == 3), "Should include the room with Id 3");
            _contextMock.Verify(c => c.Rooms, Times.Once());
        }

        /// <summary>
        /// TC-ROOM-010: Verifies that no rooms are returned when there are no matches.
        /// </summary>
        [Test]
        public async Task SearchAsync_NoMatchingCriteria_ReturnsEmptyList()
        {
            string? type = "Penthouse"; 
            decimal? minPrice = null;  
            decimal? maxPrice = null;   
            bool? available = null;      

            var result = await _roomRepository.SearchAsync(type, minPrice, maxPrice, available);

            Console.WriteLine($"Result count: {result.Count()}");

            Assert.IsNotNull(result, "The result should not be null"); 
            Assert.AreEqual(0, result.Count(), "Should return 0 rooms"); 
            _contextMock.Verify(c => c.Rooms, Times.Once()); 
        }
    }
}