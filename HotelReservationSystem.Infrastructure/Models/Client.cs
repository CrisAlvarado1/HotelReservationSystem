using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem.Infrastructure.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(100)]
        public string LastName { get; set; }

        [Required, MaxLength(50)]
        public string Email { get; set; }

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; }

    }
}
