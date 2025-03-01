using HotelReservationSystem.Infrastructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelReservationSystem.Infrastructure.Interfaces
{
    public interface IReservationRepository
    {
        Task<Reservation> AddAsync(Reservation reservation);
        Task<IEnumerable<Reservation>> GetUserReservationHistoryAsync(int clientId);
    }
}
