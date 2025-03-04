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
    public class ReservationRepositoryCancelTests
    {
        private HotelDbContext _context;
        private IReservationRepository _reservationRepository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<HotelDbContext>()
                .UseInMemoryDatabase(databaseName: "HotelReservationTest_Cancel")
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
        public async Task UpdateAsync_ExistingReservation_CancelsSuccessfully()
        {
            var reservation = new Reservation
            {
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Status = ReservationStatus.Confirmed
            };
            await _context.Reservations.AddAsync(reservation);
            await _context.SaveChangesAsync();

            reservation.Status = ReservationStatus.Canceled;

            await _reservationRepository.UpdateAsync(reservation);

            var updatedReservation = await _context.Reservations.FindAsync(reservation.Id);
            Assert.IsNotNull(updatedReservation);
            Assert.AreEqual(ReservationStatus.Canceled, updatedReservation.Status);
        }

        [Test]
        public async Task UpdateAsync_NonExistingReservation_ThrowsException()
        {
            var reservation = new Reservation
            {
                Id = 999, // ID inexistente
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Status = ReservationStatus.Canceled
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