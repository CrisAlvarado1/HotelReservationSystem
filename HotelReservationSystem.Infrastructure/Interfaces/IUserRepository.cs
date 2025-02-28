using HotelReservationSystem.Infrastructure.Models;

namespace HotelReservationSystem.Infrastructure.Interfaces
{
    public interface IUserRepository
    {
        Task<User> AddAsync(User user);
    }
}
