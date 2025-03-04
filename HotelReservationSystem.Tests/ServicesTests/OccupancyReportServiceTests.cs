using HotelReservationSystem.Core.Services;
using HotelReservationSystem.Infrastructure.Interfaces;
using Moq;

namespace HotelReservationSystem.Tests;

public class OccupancyReportServiceTests
{
    private Mock<IOccupancyReportRepository> _occupancyReportRepositoryMock;
    private OccupancyReportService _occupancyReportService;

    [SetUp]
    public void Setup()
    {
        _occupancyReportRepositoryMock = new Mock<IOccupancyReportRepository>();
        _occupancyReportService = new OccupancyReportService(_occupancyReportRepositoryMock.Object);
    }

    /// <summary>
    /// TC-OR-001 - Test to verify that the occupancy report is generated successfully.
    /// </summary>
    [Test]
    public async Task GenerateOccupancyReport_ValidDateRange_ShouldReturnOccupancyData()
    {
        // Arrange
        DateTime startDate = DateTime.Now.AddDays(5);
        DateTime endDate = DateTime.Now.AddDays(10);

        var expectedOccupancyData = new Dictionary<string, double>
            {
                { "Single", 75.5 },
                { "Double", 60.0 }
            };

        _occupancyReportRepositoryMock
            .Setup(repo => repo.GetOccupancyRateAsync(startDate, endDate))
            .ReturnsAsync(expectedOccupancyData);

        // Act
        var result = await _occupancyReportService.GenerateOccupancyReportAsync(startDate, endDate);

        // Assert
        Assert.IsNotNull(result, "The occupancy report should not be null.");
        Assert.AreEqual(2, result.Count, "The occupancy report should contain two room types.");
        Assert.AreEqual(75.5, result["Single"], "The occupancy rate for Single rooms should match.");
        Assert.AreEqual(60.0, result["Double"], "The occupancy rate for Double rooms should match.");
    }

    /// <summary>
    /// TC-OR-002 - Test to verify that the occupancy report is generated successfully when no reservations exist.
    /// </summary>
    [Test]
    public async Task GenerateOccupancyReport_NoReservations_ShouldReturnEmptyReport()
    {
        // Arrange
        DateTime startDate = DateTime.Now.AddDays(5);
        DateTime endDate = DateTime.Now.AddDays(10);

        var expectedEmptyReport = new Dictionary<string, double>(); // Empty dictionary

        _occupancyReportRepositoryMock
            .Setup(repo => repo.GetOccupancyRateAsync(startDate, endDate))
            .ReturnsAsync(expectedEmptyReport);

        // Act
        var result = await _occupancyReportService.GenerateOccupancyReportAsync(startDate, endDate);

        // Assert
        Assert.IsNotNull(result, "The occupancy report should not be null.");
        Assert.IsEmpty(result, "The occupancy report should be empty when no reservations exist.");
    }

    /// <summary>
    /// TC-OR-003 - Test to verify that an exception is thrown when an invalid date range is provided.
    /// </summary>
    [Test]
    public void GenerateOccupancyReport_InvalidDateRange_ShouldThrowException()
    {
        // Arrange
        DateTime startDate = DateTime.Now.AddDays(5);
        DateTime endDate = DateTime.Now.AddDays(3); // End date is before start date

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _occupancyReportService.GenerateOccupancyReportAsync(startDate, endDate));

        // Assert
        Assert.That(exception.Message, Is.EqualTo("Start date must be before end date."));
    }
}
