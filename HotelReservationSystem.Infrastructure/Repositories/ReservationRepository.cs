using HotelReservationSystem.Infrastructure.Data;
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
            _context = context;
        }

        public async Task<Reservation> AddAsync(Reservation reservation)
        {
            await _context.Reservations.AddAsync(reservation);

            await _context.SaveChangesAsync();

            return reservation;
        }

        public async Task<IEnumerable<Reservation>> GetUserReservationHistoryAsync(int clientId)
        {
            return await _context.Reservations
                .Include(r => r.Room) 
                .Where(r => r.ClientId == clientId)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();
        }
    }
