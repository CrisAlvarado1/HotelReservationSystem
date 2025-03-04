using HotelReservationSystem.Core.Interfaces;
using HotelReservationSystem.Infrastructure.Interfaces;

namespace HotelReservationSystem.Core.Services
{
    public class OccupancyReportService : IOccupancyReportService
    {
        private readonly IOccupancyReportRepository _occupancyReportRepository;

        public OccupancyReportService(IOccupancyReportRepository occupancyReportRepository)
        {
            _occupancyReportRepository = occupancyReportRepository;
        }

        public async Task<Dictionary<string, double>> GenerateOccupancyReportAsync(DateTime startDate, DateTime endDate)
        {
            ValidateDates(startDate, endDate);

            var occupancyData = await _occupancyReportRepository.GetOccupancyRateAsync(startDate, endDate);

            return occupancyData ?? new Dictionary<string, double>();
        }

        private void ValidateDates(DateTime startDate, DateTime endDate)
        {
            if (startDate <= DateTime.Now.Date)
                throw new ArgumentException("Start date must be today or in the future.");

            if (startDate >= endDate)
                throw new ArgumentException("Start date must be before end date.");
        }
    }
}
