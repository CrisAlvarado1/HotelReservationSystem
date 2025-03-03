
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservationSystem.Infrastructure.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReservationId { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        public int NightsStayed { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RoomPricePerNight { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        public Reservation Reservation { get; set; } = null!;
    }
}

