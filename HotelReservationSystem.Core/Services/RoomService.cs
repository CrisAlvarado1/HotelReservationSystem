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
            var rooms = await _roomRepository.SearchAsync(type, minPrice, maxPrice, available);

            return rooms.Where(r => (string.IsNullOrEmpty(type) || r.Type.Contains(type))
                                 && (!minPrice.HasValue || r.PricePerNight >= minPrice.Value)
                                 && (!maxPrice.HasValue || r.PricePerNight <= maxPrice.Value)
                                 && (!available.HasValue || r.Available == available.Value));
        }
    }
}
