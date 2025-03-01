using HotelReservationSystem.Infrastructure.Models;

namespace HotelReservationSystem.Infrastructure.Interfaces
{
    public interface IRoomRepository
    {
        Task<Room> AddAsync(Room room);

        Task<Room> FindByIdAsync(int id);

        Task<IEnumerable<Room>> SearchAsync(string? type, decimal? minPrice, decimal? maxPrice, bool? available);
      
        Task<bool> IsRoomAvailable(int romId, DateTime startDate, DateTime endDate);

        Task UpdateAvailabilityAsync(int roomId, bool available);
    }
}
