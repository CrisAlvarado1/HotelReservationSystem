using HotelReservationSystem.Infrastructure.Models;

namespace HotelReservationSystem.Core.Interfaces
{
    public interface IReservationService
    {
        Task<Reservation> ReserveRoomAsync(Reservation reservation);

        Task CancelReservationAsync(int reservationId);

    }
}
