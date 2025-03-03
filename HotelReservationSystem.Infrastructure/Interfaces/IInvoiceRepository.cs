using HotelReservationSystem.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystem.Infrastructure.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<Invoice> AddAsync(Invoice invoice);
        Task<Invoice> GetByReservationIdAsync(int reservationId);
    }
}
