using System;
using Microsoft.EntityFrameworkCore;
using HotelReservationSystem.Infrastructure.Models;

namespace HotelReservationSystem.Infrastructure.Data
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }

        public virtual DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public  virtual DbSet<Reservation> Reservations { get; set; }
        public DbSet<Room> Rooms { get; set; }
    }
}