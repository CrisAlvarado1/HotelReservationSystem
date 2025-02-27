using HotelReservationSystem.Infrastructure.Models;

namespace HotelReservationSystem.Core.Interfaces
{
    public interface IRoomService
    {
        Task<Room> RegisterRoomAsync(Room room);
    }
}
