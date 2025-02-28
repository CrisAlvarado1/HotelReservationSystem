using System.ComponentModel.DataAnnotations;
using HotelReservationSystem.Core.Interfaces;
using HotelReservationSystem.Infrastructure.Models;
using System.Text.RegularExpressions;
using HotelReservationSystem.Infrastructure.Interfaces;

namespace HotelReservationSystem.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> RegisterUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "The user cannot be null.");

            if (string.IsNullOrWhiteSpace(user.Name))
                throw new ValidationException("The user name is required.");

            if (string.IsNullOrWhiteSpace(user.LastName))
                throw new ValidationException("The user last name is required.");

            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ValidationException("The user email is required.");

            if (!IsValidEmail(user.Email))
                throw new ValidationException("The user email is not valid.");

            var registeredUser = await _userRepository.AddAsync(user);
            return registeredUser;
        }

        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$"); 
            return emailRegex.IsMatch(email);
        }
    }
}
