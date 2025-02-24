using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HotelReservationSystem.Infrastructure.Data.Enum;

namespace HotelReservationSystem.Infrastructure.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Other;

        public Invoice Invoice { get; set; } = null!;
    }
}
