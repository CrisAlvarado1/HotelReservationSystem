using HotelReservationSystem.Infrastructure.Models;

namespace HotelReservationSystem.Infrastructure.Interfaces
{
    public interface IRoomRepository
    {
        Task<Room> AddAsync(Room room);
    }
}
