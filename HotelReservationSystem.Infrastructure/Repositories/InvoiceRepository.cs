using HotelReservationSystem.Infrastructure.Data;
using HotelReservationSystem.Infrastructure.Interfaces;
using HotelReservationSystem.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationSystem.Infrastructure.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly HotelDbContext _context;

        public InvoiceRepository(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<Invoice> AddAsync(Invoice invoice)
        {
            await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<Invoice> GetByReservationIdAsync(int reservationId)
        {
            return await _context.Invoices
                .FirstOrDefaultAsync(i => i.ReservationId == reservationId);
        }
    }
}
