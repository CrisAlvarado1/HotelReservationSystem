using System.ComponentModel.DataAnnotations;
using HotelReservationSystem.Infrastructure.Data.Enum;

namespace HotelReservationSystem.Infrastructure.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(100)]
        public string LastName { get; set; }

        [Required, MaxLength(50)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public UserType UserType { get; set; } = UserType.Client;

        public ICollection<Reservation>? Reservations { get; set; } // If the user is a client
    }
}
