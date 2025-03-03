using HotelReservationSystem.Core.Services;
using HotelReservationSystem.Infrastructure.Data.Enum;
using HotelReservationSystem.Infrastructure.Interfaces;
using HotelReservationSystem.Infrastructure.Models;
using Moq;


namespace HotelReservationSystem.Tests
{
    [TestFixture]
    public class InvoiceServiceTests
    {
        private Mock<IReservationRepository> _reservationRepositoryMock;
        private Mock<IInvoiceRepository> _invoiceRepositoryMock;
        private InvoiceService _invoiceService;

        [SetUp]
        public void Setup()
        {
            _reservationRepositoryMock = new Mock<IReservationRepository>();
            _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            _invoiceService = new InvoiceService(_reservationRepositoryMock.Object, _invoiceRepositoryMock.Object);
        }

        /// <summary>
        /// TC-IS-001 - Test to verify that an invoice is generated successfully for a valid reservation.
        /// </summary>
        [Test]
        public async Task GenerateInvoiceAsync_ValidReservation_ReturnsInvoice()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = 1,
                Status = ReservationStatus.Confirmed,
                StartDate = DateTime.UtcNow.AddDays(-2),
                EndDate = DateTime.UtcNow.AddDays(-1),
                Room = new Room { PricePerNight = 100 }
            };
            _reservationRepositoryMock.Setup(r => r.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(reservation);

            var invoice = new Invoice
            {
                ReservationId = 1,
                IssueDate = DateTime.UtcNow,
                NightsStayed = 1,
                RoomPricePerNight = 100,
                TotalAmount = 100
            };
            _invoiceRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Invoice>())).ReturnsAsync(invoice);

            // Act
            var result = await _invoiceService.GenerateInvoiceAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ReservationId);
            Assert.AreEqual(100, result.TotalAmount);
            _invoiceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Once);
        }

        /// <summary>
        /// TC-IS-002 - Test to verify that an InvalidOperationException is thrown when the reservation is not found.
        /// </summary>
        [Test]
        public void GenerateInvoiceAsync_NonExistingReservation_ThrowsInvalidOperationException()
        {
            // Arrange
            _reservationRepositoryMock.Setup(r => r.FindByIdAsync(It.IsAny<int>())).ReturnsAsync((Reservation)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _invoiceService.GenerateInvoiceAsync(999));

            Assert.AreEqual("Reservation not found.", ex.Message);
        }

        /// <summary>
        /// TC-IS-003 - Test to verify that an InvalidOperationException is thrown when the reservation is not confirmed.
        /// </summary>
        [Test]
        public void GenerateInvoiceAsync_NonConfirmedReservation_ThrowsInvalidOperationException()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = 1,
                Status = ReservationStatus.Canceled // Usamos Canceled en lugar de Pending
            };
            _reservationRepositoryMock.Setup(r => r.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(reservation);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _invoiceService.GenerateInvoiceAsync(1));

            Assert.AreEqual("Only confirmed reservations can generate invoices.", ex.Message);
        }

        /// <summary>
        /// TC-IS-004 - Test to verify that an InvalidOperationException is thrown when the check-out date has not passed.
        /// </summary>
        [Test]
        public void GenerateInvoiceAsync_BeforeCheckOut_ThrowsInvalidOperationException()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = 1,
                Status = ReservationStatus.Confirmed,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(2)
            };
            _reservationRepositoryMock.Setup(r => r.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(reservation);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _invoiceService.GenerateInvoiceAsync(1));

            Assert.AreEqual("Invoice can only be generated after check-out.", ex.Message);
        }

        /// <summary>
        /// TC-IS-005 - Test to verify that the system correctly calculates the number of nights stayed.
        /// </summary>
        [Test]
        public async Task GenerateInvoiceAsync_ValidReservation_CalculatesNightsStayedCorrectly()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = 1,
                Status = ReservationStatus.Confirmed,
                StartDate = DateTime.UtcNow.AddDays(-5),
                EndDate = DateTime.UtcNow.AddDays(-2),
                Room = new Room { PricePerNight = 200 }
            };
            _reservationRepositoryMock.Setup(r => r.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(reservation);

            var invoice = new Invoice
            {
                ReservationId = 1,
                IssueDate = DateTime.UtcNow,
                NightsStayed = 3,
                RoomPricePerNight = 200,
                TotalAmount = 600
            };
            _invoiceRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Invoice>())).ReturnsAsync(invoice);

            // Act
            var result = await _invoiceService.GenerateInvoiceAsync(1);

            // Assert
            Assert.AreEqual(3, result.NightsStayed);
        }

        /// <summary>
        /// TC-IS-006 - Test to verify that the system correctly calculates the total amount of the invoice.
        /// </summary>
        [Test]
        public async Task GenerateInvoiceAsync_ValidReservation_CalculatesTotalAmountCorrectly()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = 1,
                Status = ReservationStatus.Confirmed,
                StartDate = DateTime.UtcNow.AddDays(-3),
                EndDate = DateTime.UtcNow.AddDays(-1),
                Room = new Room { PricePerNight = 150 }
            };
            _reservationRepositoryMock.Setup(r => r.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(reservation);

            var invoice = new Invoice
            {
                ReservationId = 1,
                IssueDate = DateTime.UtcNow,
                NightsStayed = 2,
                RoomPricePerNight = 150,
                TotalAmount = 300
            };
            _invoiceRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Invoice>())).ReturnsAsync(invoice);

            // Act
            var result = await _invoiceService.GenerateInvoiceAsync(1);

            // Assert
            Assert.AreEqual(300, result.TotalAmount);
        }

        /// <summary>
        /// TC-IS-007 - Test to verify that an ArgumentNullException is thrown when the invoice is null.
        /// </summary>
        [Test]
        public void AddAsync_NullInvoice_ThrowsArgumentNullException()
        {
            // Arrange
            Invoice nullInvoice = null;
            _invoiceRepositoryMock.Setup(r => r.AddAsync(nullInvoice))
                                  .ThrowsAsync(new ArgumentNullException(nameof(nullInvoice), "The invoice cannot be null."));

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _invoiceRepositoryMock.Object.AddAsync(nullInvoice));

            // Verifica el mensaje completo de la excepción
            Assert.AreEqual("The invoice cannot be null. (Parameter 'nullInvoice')", ex.Message);
        }

        /// <summary>
        /// TC-IS-008 - Test to verify that an ArgumentException is thrown when the reservation ID is invalid.
        /// </summary>
        [Test]
        public void GetByReservationIdAsync_InvalidReservationId_ThrowsArgumentException()
        {
            // Arrange
            int invalidReservationId = 0;
            _invoiceRepositoryMock.Setup(r => r.GetByReservationIdAsync(invalidReservationId))
                                  .ThrowsAsync(new ArgumentException("Reservation ID must be greater than zero.", nameof(invalidReservationId)));

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _invoiceRepositoryMock.Object.GetByReservationIdAsync(invalidReservationId));

            // Verifica el mensaje completo de la excepción
            Assert.AreEqual("Reservation ID must be greater than zero. (Parameter 'invalidReservationId')", ex.Message);
        }

        /// <summary>
        /// TC-IS-009 - Test to verify that an InvalidOperationException is thrown when no invoice is found for the reservation ID.
        /// </summary>
        [Test]
        public void GetByReservationIdAsync_NonExistingInvoice_ThrowsInvalidOperationException()
        {
            // Arrange
            int reservationId = 999;
            _invoiceRepositoryMock.Setup(r => r.GetByReservationIdAsync(reservationId))
                                  .ThrowsAsync(new InvalidOperationException($"No invoice found for reservation ID {reservationId}."));

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _invoiceRepositoryMock.Object.GetByReservationIdAsync(reservationId));

            Assert.AreEqual($"No invoice found for reservation ID {reservationId}.", ex.Message);
        }
    }
}