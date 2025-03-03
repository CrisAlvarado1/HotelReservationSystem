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
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice), "The invoice cannot be null.");

            await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<Invoice> GetByReservationIdAsync(int reservationId)
        {
            if (reservationId <= 0)
                throw new ArgumentException("Reservation ID must be greater than zero.", nameof(reservationId));

            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.ReservationId == reservationId);

            if (invoice == null)
                throw new InvalidOperationException($"No invoice found for reservation ID {reservationId}.");

            return invoice;
        }
    }
}
