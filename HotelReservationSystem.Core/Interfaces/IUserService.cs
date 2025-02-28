using HotelReservationSystem.Infrastructure.Models;

namespace HotelReservationSystem.Core.Interfaces
{
    public interface IUserService
    {
        Task<User> RegisterUserAsync(User user);
    }
}