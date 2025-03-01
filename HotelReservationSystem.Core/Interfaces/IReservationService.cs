using HotelReservationSystem.Infrastructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelReservationSystem.Core.Interfaces
{
    public interface IReservationService
    {
        Task<Reservation> ReserveRoomAsync(Reservation reservation);
      
        Task<IEnumerable<Reservation>> ReservationHistoryAsync(int userId);
      
        Task CancelReservationAsync(int reservationId);
    }
}