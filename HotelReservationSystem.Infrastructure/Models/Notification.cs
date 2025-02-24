using System;
using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem.Infrastructure.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public DateTime SentDate { get; set; }

        // Navigation Property
        public Client Client { get; set; } = null!;
    }
}