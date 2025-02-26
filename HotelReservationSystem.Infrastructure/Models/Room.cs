using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem.Infrastructure.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Type { get; set; }

        [Required]
        public decimal PricePerNight { get; set; }

        [Required]
        public bool Available { get; set; }

        public ICollection<Reservation> Reservas { get; set; } = new List<Reservation>();
    }
}
