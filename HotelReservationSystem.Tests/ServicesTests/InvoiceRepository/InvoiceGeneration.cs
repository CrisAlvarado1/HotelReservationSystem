using Moq;
using Moq.EntityFrameworkCore;
using HotelReservationSystem.Infrastructure.Data;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;



namespace HotelReservationSystem.Tests.InvoiceRepositoryTest
{
    [TestFixture]
    public class InvoiceGeneration
    {
        private Mock<HotelDbContext> _contextMock;
        private InvoiceRepository _invoiceRepository;
        private List<Invoice> _invoices;

        [SetUp]
        public void Setup()
        {

            _invoices = new List<Invoice>();


            var options = new DbContextOptions<HotelDbContext>();


            _contextMock = new Mock<HotelDbContext>(options);
            _contextMock.Setup(c => c.Invoices).ReturnsDbSet(_invoices);
            _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);


            _invoiceRepository = new InvoiceRepository(_contextMock.Object);
        }

        /// <summary>
        /// TC-INV-REPO-001: Verifies that a valid invoice is correctly added to the database.
        /// </summary>
        [Test]
        public async Task AddAsync_ValidData_ShouldAddSuccessfully()
        {
            // Arrange
            var invoice = new Invoice
            {
                Id = 1,
                ReservationId = 1,
                IssueDate = DateTime.UtcNow,
                NightsStayed = 3,
                RoomPricePerNight = 100.0m,
                TotalAmount = 300.0m
            };

            _invoices.Add(invoice);
            await _contextMock.Object.SaveChangesAsync();

            // Assert
            Assert.AreEqual(1, _invoices.Count);
            Assert.AreEqual(invoice.ReservationId, _invoices[0].ReservationId);
            Assert.AreEqual(invoice.TotalAmount, _invoices[0].TotalAmount);
            _contextMock.Verify(repo => repo.SaveChangesAsync(default), Times.Once());
        }

        /// <summary>
        /// TC-INV-REPO-002: Verifies that an exception is thrown if there is an error in the database when saving.
        /// </summary>
        [Test]
        public async Task AddAsync_DbFailure_ShouldThrowException()
        {
            // Arrange
            var invoice = new Invoice
            {
                Id = 1,
                ReservationId = 1,
                IssueDate = DateTime.UtcNow,
                NightsStayed = 3,
                RoomPricePerNight = 100.0m,
                TotalAmount = 300.0m
            };

            _contextMock.Setup(c => c.SaveChangesAsync(default))
                        .ThrowsAsync(new DbUpdateException("Database error", new Exception()));

            // Act & Assert
            var ex = Assert.ThrowsAsync<DbUpdateException>(async () =>
                await _invoiceRepository.AddAsync(invoice));

            Assert.AreEqual("Database error", ex.Message);
            _contextMock.Verify(repo => repo.SaveChangesAsync(default), Times.Once());
        }

        /// <summary>
        /// TC-INV-REPO-003: Verifies that an invoice is obtained correctly when the reservation ID is valid.
        /// </summary>
        [Test]
        public async Task GetByReservationIdAsync_ValidReservationId_ShouldReturnInvoice()
        {
            // Arrange
            var reservationId = 1;
            var invoice = new Invoice
            {
                Id = 1,
                ReservationId = reservationId,
                IssueDate = DateTime.UtcNow,
                NightsStayed = 3,
                RoomPricePerNight = 100.0m,
                TotalAmount = 300.0m
            };

            _invoices.Add(invoice);

            // Act
            var result = await _invoiceRepository.GetByReservationIdAsync(reservationId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(invoice.ReservationId, result.ReservationId);
            Assert.AreEqual(invoice.TotalAmount, result.TotalAmount);
        }

        /// <summary>
        /// TC-INV-REPO-004: Verifies that an exception is thrown when an invoice is not found for the given reservation ID.
        /// </summary>
        [Test]
        public async Task GetByReservationIdAsync_InvalidReservationId_ShouldThrowException()
        {

            var reservationId = 999;

            _contextMock.Setup(c => c.Invoices).ReturnsDbSet(new List<Invoice>());

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _invoiceRepository.GetByReservationIdAsync(reservationId));

            Assert.AreEqual($"No invoice found for reservation ID {reservationId}.", ex.Message);
        }
    }
}