using HotelReservationSystem.Infrastructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelReservationSystem.Infrastructure.Interfaces
{
    public interface IReservationRepository
    {
        Task<Reservation> AddAsync(Reservation reservation);

        Task<IEnumerable<Reservation>> GetUserReservationHistoryAsync(int clientId);

        Task<Reservation> FindByIdAsync(int id);

        Task UpdateAsync(Reservation reservation);

        Task<bool> HasConfirmedReservationsAsync(int roomId, DateTime startDate, DateTime endDate, int? excludeReservationId = null);
    }
}
