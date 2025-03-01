using HotelReservationSystem.Infrastructure.Data;
using HotelReservationSystem.Infrastructure.Interfaces;
using HotelReservationSystem.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationSystem.Infrastructure.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly HotelDbContext _context;

        public RoomRepository(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<Room> AddAsync(Room room)
        {
            await _context.Rooms.AddAsync(room);
            await _context.SaveChangesAsync();

            return room;
        }

        public async Task<Room> FindByIdAsync(int id)
        {
            return await _context.Rooms.FindAsync(id);
        }


        public async Task<IEnumerable<Room>> SearchAsync(string? type, decimal? minPrice, decimal? maxPrice, bool? available)
        {
            var query = _context.Rooms.AsQueryable();

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(r => r.Type.Contains(type));
            }
            if (minPrice.HasValue)
            {
                query = query.Where(r => r.PricePerNight >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(r => r.PricePerNight <= maxPrice.Value);
            }
            if (available.HasValue)
            {
                query = query.Where(r => r.Available == available.Value);
            }

            return await query.ToListAsync();

        public async Task<bool> IsRoomAvailable(int roomId, DateTime startDate, DateTime endDate)
        {
            var room = await _context.Rooms
                .Include(r => r.Reservas)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if(room == null || !room.Available)
            {
                return false;
            }

            bool isOverlapping = room.Reservas.Any(reservation =>
            reservation.Status == HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Confirmed &&
            startDate < reservation.EndDate && endDate > reservation.StartDate);

            return !isOverlapping;
        }

        public async Task UpdateAvailabilityAsync(int roomId, bool available)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
                throw new InvalidOperationException($"Room with ID {roomId} not found.");

            room.Available = available;

            await _context.SaveChangesAsync();
        }
    }
}
