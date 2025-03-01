using System.ComponentModel.DataAnnotations;
using HotelReservationSystem.Core.Interfaces;
using HotelReservationSystem.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;


namespace HotelReservationSystem.API.Controllers
{
    [Route("hotel-reservation/[controller]")]

    [ApiController]

    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpPost]
        public async Task<IActionResult> reserveRoomAsync([FromBody] Reservation reservation)
        {
            try
            {
                var registeredReservation = await _reservationService.ReserveRoomAsync(reservation);

                return Ok(registeredReservation);
            }
            catch (ArgumentNullException Ex)
            {
                return BadRequest(Ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("user/{userId}/history")]
        public async Task<IActionResult> GetUserReservationHistoryAsync([FromRoute] int userId)
        {
            try
            {
                var reservationHistory = await _reservationService.ReservationHistoryAsync(userId);

                if (reservationHistory == null || !reservationHistory.Any())
                {
                    return NotFound($"No reservations found for user ID {userId}.");
                }

                return Ok(reservationHistory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); 
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message); 

            }
        }
    }
}
    

