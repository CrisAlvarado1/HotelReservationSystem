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
            // Validar datos de entrada
            if (room == null)
                throw new ArgumentNullException(nameof(room), "The room cannot be null.");

            if (string.IsNullOrWhiteSpace(room.Type))
                throw new ValidationException("The type of room is mandatory.");

            if (room.PricePerNight <= 0)
                throw new ArgumentException("The price per night must be greater than zero.");

            // Registrar la habitación
            var registeredRoom = await _roomRepository.AddAsync(room);
            return registeredRoom;
        }
    }
}
