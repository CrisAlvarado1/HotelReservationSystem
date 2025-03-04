using HotelReservationSystem.Core.Interfaces;
using HotelReservationSystem.Infrastructure.Data.Enum;
using HotelReservationSystem.Infrastructure.Interfaces;
using HotelReservationSystem.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystem.Core.Services
{
    public class InvoiceService: IInvoiceService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceService(IReservationRepository reservationRepository, IInvoiceRepository invoiceRepository)
        {
            _reservationRepository = reservationRepository;
            _invoiceRepository = invoiceRepository;
        }

        public async Task<Invoice> GenerateInvoiceAsync(int reservationId)
        {
            var reservation = await _reservationRepository.FindByIdAsync(reservationId);
            if (reservation == null)
                throw new InvalidOperationException("Reservation not found.");

            if (reservation.Status != ReservationStatus.Confirmed)
                throw new InvalidOperationException("Only confirmed reservations can generate invoices.");

            if (reservation.EndDate > DateTime.UtcNow)
                throw new InvalidOperationException("Invoice can only be generated after check-out.");

            int nightsStayed = CalculateNightsStayed(reservation.StartDate, reservation.EndDate);

            decimal totalAmount = CalculateTotalAmount(nightsStayed, reservation.Room.PricePerNight);

            var invoice = new Invoice
            {
                ReservationId = reservationId,
                IssueDate = DateTime.UtcNow,
                NightsStayed = nightsStayed,
                RoomPricePerNight = reservation.Room.PricePerNight,
                TotalAmount = totalAmount
            };

            return await _invoiceRepository.AddAsync(invoice);
        }

        private int CalculateNightsStayed(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new InvalidOperationException("Start date cannot be later than end date.");

            return (endDate - startDate).Days;
        }

        private decimal CalculateTotalAmount(int nightsStayed, decimal pricePerNight)
        {
            if (nightsStayed <= 0)
                throw new InvalidOperationException("Nights stayed must be greater than zero.");

            if (pricePerNight <= 0)
                throw new InvalidOperationException("Price per night must be greater than zero.");

            return nightsStayed * pricePerNight;
        }
    }
}