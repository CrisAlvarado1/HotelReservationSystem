﻿using HotelReservationSystem.Infrastructure.Data;
using HotelReservationSystem.Infrastructure.Interfaces;
using HotelReservationSystem.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelReservationSystem.Infrastructure.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly HotelDbContext _context;

        public ReservationRepository(HotelDbContext context)
        {
            this._context = context;
        }

        public async Task<Reservation> AddAsync(Reservation reservation)
        {
            await _context.Reservations.AddAsync(reservation);

            await _context.SaveChangesAsync();

            return reservation;
        }

        public async Task<IEnumerable<Reservation>> GetUserReservationHistoryAsync(int userId)
        {
            return await _context.Reservations
                .Include(r => r.Room)
                .Where(r => r.ClientId == userId)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();
        }

        public async Task<Reservation> FindByIdAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task UpdateAsync(Reservation reservation)
        {
            _context.Reservations.Update(reservation);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasConfirmedReservationsAsync(int roomId, DateTime startDate, DateTime endDate, int? excludeReservationId = null)
        {
            return await _context.Reservations
                .Where(r => r.RoomId == roomId &&
                r.Status == HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Confirmed &&
                (excludeReservationId == null || r.Id != excludeReservationId) &&
                startDate < r.EndDate && endDate > r.StartDate).AnyAsync();
        }

        public async Task<List<Reservation>> FindReservationsByStartDateRangeAsync(DateTime startRange, DateTime endRange)
        {
            return await _context.Reservations
                .Where(r => r.StartDate >= startRange && r.StartDate <= endRange
                         && r.Status == HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Confirmed)
                .ToListAsync();
        }
    }
}
