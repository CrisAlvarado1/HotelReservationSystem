using HotelReservationSystem.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservationSystem.API.Controllers
{
    [Route("hotel-reservation/[controller]")]
    [ApiController]
    public class OccupancyReportController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetOccupancyReportAsync([FromServices] IOccupancyReportService occupancyReportService, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var occupancyReport = await occupancyReportService.GenerateOccupancyReportAsync(startDate, endDate);
                return Ok(occupancyReport);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
