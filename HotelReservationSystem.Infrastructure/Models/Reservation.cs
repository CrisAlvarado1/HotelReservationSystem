using HotelReservationSystem.Infrastructure.Data.Enum;
using System.ComponentModel.DataAnnotations;


namespace HotelReservationSystem.Infrastructure.Models
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed; // Default Value

        public User? Client { get; set; }
        public Room? Room { get; set; }
    }
}
