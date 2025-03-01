using HotelReservationSystem.Infrastructure.Models;

namespace HotelReservationSystem.Infrastructure.Interfaces
{
    public interface IReservationRepository
    {
        Task<Reservation> AddAsync(Reservation reservation);
        Task<IEnumerable<Reservation>> GetUserReservationHistoryAsync(int clientId);
    }
}
