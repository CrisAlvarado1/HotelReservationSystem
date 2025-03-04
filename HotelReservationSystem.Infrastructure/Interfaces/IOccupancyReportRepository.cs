using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystem.Infrastructure.Interfaces
{
    public interface IOccupancyReportRepository
    {
        Task<Dictionary<string, double>> GetOccupancyRateAsync(DateTime startDate, DateTime endDate);
        Task<int> GetTotalRoomsByTypeAsync(string roomType);
    }
}
