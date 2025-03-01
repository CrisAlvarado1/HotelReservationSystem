using HotelReservationSystem.Infrastructure.Models;

namespace HotelReservationSystem.Core.Interfaces
{
    public interface IRoomService
    {
        Task<Room> RegisterRoomAsync(Room room);

        Task<IEnumerable<Room>> SearchAsync(string? type, decimal? minPrice, decimal? maxPrice, bool? available);
    }
}
