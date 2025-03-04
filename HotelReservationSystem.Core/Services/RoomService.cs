using System.ComponentModel.DataAnnotations;
using HotelReservationSystem.Infrastructure.Interfaces;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Core.Interfaces;

namespace HotelReservationSystem.Core.Services
{
    public class RoomService : IRoomService
    {

        private readonly IRoomRepository _roomRepository;

        public RoomService(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<Room> RegisterRoomAsync(Room room)
        {
            if (room == null)
                throw new ArgumentNullException(nameof(room), "The room cannot be null.");

            if (string.IsNullOrWhiteSpace(room.Type))
                throw new ValidationException("The room type is required.");

            if (room.PricePerNight <= 0)
                throw new ArgumentException("The price per night must be greater than zero.");

            if (await _roomRepository.FindByIdAsync(room.Id) != null)
                throw new InvalidOperationException("A room with the same number already exists.");

            var registeredRoom = await _roomRepository.AddAsync(room);
            return registeredRoom;
        }

        public async Task<IEnumerable<Room>> SearchAsync(string? type, decimal? minPrice, decimal? maxPrice, bool? available)
        {
            ValidatePriceRange(minPrice, maxPrice);

            var rooms = await _roomRepository.SearchAsync(type, minPrice, maxPrice, available);

            return rooms.Where(r => (string.IsNullOrEmpty(type) || r.Type.Contains(type))
                                 && (!minPrice.HasValue || r.PricePerNight >= minPrice.Value)
                                 && (!maxPrice.HasValue || r.PricePerNight <= maxPrice.Value)
                                 && (!available.HasValue || r.Available == available.Value));
        }

        private static void ValidatePriceRange(decimal? minPrice, decimal? maxPrice)
        {
            if (minPrice.HasValue && minPrice.Value < 0)
                throw new ArgumentException("Invalid price range provided.");

            if (maxPrice.HasValue && maxPrice.Value < 0)
                throw new ArgumentException("Invalid price range provided.");

            if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
                throw new ArgumentException("Invalid price range provided.");
        }

        public async Task<IEnumerable<Room>> CheckAvailabilityAsync(DateTime startDate, DateTime endDate)
        {
            ValidateDates(startDate, endDate);

            var rooms = await _roomRepository.GetAvailableRoomsAsync(startDate, endDate);

            return rooms ?? new List<Room>();
        }

        private void ValidateDates(DateTime startDate, DateTime endDate)
        {
            if (startDate <= DateTime.Now.Date)
                throw new ArgumentException("Start date must be today or in the future.");

            if (startDate >= endDate)
                throw new ArgumentException("Start date must be before end date.");
        }
    }
}
