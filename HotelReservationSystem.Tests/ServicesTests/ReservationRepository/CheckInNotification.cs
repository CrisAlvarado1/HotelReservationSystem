// Agrupar las pruebas unitarias relacionadas con las notificaciones de check-in en el repositorio
using HotelReservationSystem.Infrastructure.Data;
using HotelReservationSystem.Infrastructure.Data.Enum;
using HotelReservationSystem.Infrastructure.Interfaces;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace HotelReservationSystem.Tests.RepositoryTests
{
    [TestFixture]
    public class ReservationRepositoryCheckInTests
    {
        private HotelDbContext _context;
        private IReservationRepository _reservationRepository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<HotelDbContext>()
                .UseInMemoryDatabase(databaseName: "HotelReservationTest_CheckIn")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _context = new HotelDbContext(options);
            _reservationRepository = new ReservationRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            if (_context != null)
            {
                _context.Dispose();
                _context = null;
            }
        }

        [Test]
        public async Task FindReservationsByStartDateRangeAsync_ReservationsInRange_ReturnsReservations()
        {
            var reservation1 = new Reservation
            {
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                EndDate = DateTime.UtcNow.Date.AddDays(3),
                Status = ReservationStatus.Confirmed,
                IsNotified = false
            };
            var reservation2 = new Reservation
            {
                ClientId = 2,
                RoomId = 2,
                StartDate = DateTime.UtcNow.Date.AddDays(2),
                EndDate = DateTime.UtcNow.Date.AddDays(4),
                Status = ReservationStatus.Confirmed,
                IsNotified = false
            };
            await _context.Reservations.AddRangeAsync(reservation1, reservation2);
            await _context.SaveChangesAsync();

            var result = await _reservationRepository.FindReservationsByStartDateRangeAsync(
                DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(2));

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(r => r.Status == ReservationStatus.Confirmed));
        }

        [Test]
        public async Task FindReservationsByStartDateRangeAsync_NoReservationsInRange_ReturnsEmptyList()
        {
            var result = await _reservationRepository.FindReservationsByStartDateRangeAsync(
                DateTime.UtcNow.Date.AddDays(10), DateTime.UtcNow.Date.AddDays(12));

            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task UpdateAsync_ExistingReservation_MarksAsNotifiedSuccessfully()
        {
            var reservation = new Reservation
            {
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                EndDate = DateTime.UtcNow.Date.AddDays(3),
                Status = ReservationStatus.Confirmed,
                IsNotified = false
            };
            await _context.Reservations.AddAsync(reservation);
            await _context.SaveChangesAsync();

            reservation.IsNotified = true;

            await _reservationRepository.UpdateAsync(reservation);

            var updatedReservation = await _context.Reservations.FindAsync(reservation.Id);
            Assert.IsNotNull(updatedReservation);
            Assert.IsTrue(updatedReservation.IsNotified);
        }

        [Test]
        public async Task UpdateAsync_NonExistingReservation_ThrowsException()
        {
            var reservation = new Reservation
            {
                Id = 999, // ID inexistente
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                EndDate = DateTime.UtcNow.Date.AddDays(3),
                Status = ReservationStatus.Confirmed,
                IsNotified = true
            };

            Exception caughtException = null;
            try
            {
                await _reservationRepository.UpdateAsync(reservation);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.IsNotNull(caughtException);
            Assert.IsTrue(caughtException is DbUpdateConcurrencyException || caughtException.InnerException is DbUpdateConcurrencyException);
        }
    }
}