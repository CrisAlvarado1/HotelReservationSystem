using HotelReservationSystem.Infrastructure.Models;

namespace HotelReservationSystem.Infrastructure.Interfaces
{
    public interface IReservationRepository
    {
        Task<Reservation> AddAsync(Reservation reservation);

        Task<Reservation> FindByIdAsync(int id);

        Task UpdateAsync(Reservation reservation);

        Task<bool> HasConfirmedReservationsAsync(int roomId, DateTime startDate, DateTime endDate, int? excludeReservationId = null);
    }
}
