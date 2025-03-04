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
    public class ReservationRepositoryTests
    {
        private HotelDbContext _context;
        private IReservationRepository _reservationRepository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<HotelDbContext>()
                .UseInMemoryDatabase(databaseName: "HotelReservationTest")
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
        public async Task AddAsync_ValidReservation_SavesSuccessfully()
        {
            var reservation = new Reservation
            {
                ClientId = 1,
                RoomId = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Status = ReservationStatus.Confirmed
            };

            var result = await _reservationRepository.AddAsync(reservation);

            Assert.IsNotNull(result);
            Assert.AreEqual(reservation.ClientId, result.ClientId);

            var savedReservation = await _context.Reservations.FindAsync(result.Id);
            Assert.IsNotNull(savedReservation);
            Assert.AreEqual(reservation.ClientId, savedReservation.ClientId);
        }

        [Test]
        public async Task AddAsync_NullReservation_ThrowsException()
        {
            Reservation reservation = null;

            Exception caughtException = null;
            try
            {
                await _reservationRepository.AddAsync(reservation);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.IsNotNull(caughtException);
            Assert.IsTrue(caughtException is ArgumentNullException);
        }
    }
}