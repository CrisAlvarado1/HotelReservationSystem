using HotelReservationSystem.Infrastructure.Models;

namespace HotelReservationSystem.Infrastructure.Interfaces
{
    public interface IReservationRepository
    {
        Task<Reservation> AddAsync(Reservation reservation);

        Task<Reservation> FindByIdAsync(Reservation reservation);

        Task UpdateAsync(Reservation reservation);
    }
}
