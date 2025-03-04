using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystem.Core.Interfaces
{
    public interface IOccupancyReportService
    {
        Task<Dictionary<string, double>> GenerateOccupancyReportAsync(DateTime startDate, DateTime endDate);
    }
}
