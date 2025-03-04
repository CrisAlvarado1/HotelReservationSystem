using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelReservationSystem.Infrastructure.Data;
using HotelReservationSystem.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationSystem.Infrastructure.Repositories
{
    public class OccupancyReportRepository : IOccupancyReportRepository
    {
        private readonly HotelDbContext _context;

        public OccupancyReportRepository(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, double>> GetOccupancyRateAsync(DateTime startDate, DateTime endDate)
        {
            if (!await _context.Rooms.AnyAsync())
            {
                throw new InvalidOperationException("No rooms found in the system.");
            }

            var reservations = await _context.Reservations
                .Where(r => r.StartDate < endDate && r.EndDate > startDate)
                .Join(_context.Rooms,
                      reservation => reservation.RoomId,
                      room => room.Id,
                      (reservation, room) => new { room.Type, reservation.Id })
                .GroupBy(r => r.Type)
                .Select(group => new
                {
                    RoomType = group.Key,
                    ReservationCount = group.Count()
                })
                .ToListAsync();

            var occupancyRates = new Dictionary<string, double>();

            foreach (var entry in reservations)
            {
                int totalRooms = await GetTotalRoomsByTypeAsync(entry.RoomType);
                if (totalRooms > 0)
                {
                    double occupancyRate = (double)entry.ReservationCount / totalRooms * 100;
                    occupancyRates[entry.RoomType] = Math.Round(occupancyRate, 2);
                }
            }

            return occupancyRates;
        }

        public async Task<int> GetTotalRoomsByTypeAsync(string roomType)
        {
            return await _context.Rooms.CountAsync(r => r.Type == roomType);
        }
    }
}
