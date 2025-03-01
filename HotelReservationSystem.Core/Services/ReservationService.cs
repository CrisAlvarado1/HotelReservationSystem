using System;
using System.ComponentModel.DataAnnotations;
using HotelReservationSystem.Infrastructure.Interfaces;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Core.Interfaces;
using Npgsql.Internal;


namespace HotelReservationSystem.Core.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IRoomRepository _roomRepository;

        public ReservationService(IReservationRepository reservationRepository, IRoomRepository roomRepository)
        {
            _reservationRepository = reservationRepository;
            _roomRepository = roomRepository;
        }

        // En ReservationService.cs
        public async Task<Reservation> ReserveRoomAsync(Reservation reservation)
        {
            if (reservation == null)
                throw new ArgumentNullException(nameof(reservation), "The reservation cannot be null.");

            if (reservation.StartDate >= reservation.EndDate)
                throw new ArgumentException("The start date must be earlier than the end date.");

            if (reservation.StartDate < DateTime.Now.Date)
                throw new ArgumentException("The start date cannot be in the past.");

            reservation.StartDate = DateTime.SpecifyKind(reservation.StartDate, DateTimeKind.Utc);
            reservation.EndDate = DateTime.SpecifyKind(reservation.EndDate, DateTimeKind.Utc);

            bool isAvailable = await _roomRepository.IsRoomAvailable(reservation.RoomId, reservation.StartDate, reservation.EndDate);
            if (!isAvailable)
                throw new InvalidOperationException("The room is not available for the selected dates.");

            reservation.Status = HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Confirmed;

            var registeredReservation = await _reservationRepository.AddAsync(reservation);

            await _roomRepository.UpdateAvailabilityAsync(reservation.RoomId, false);

            return registeredReservation;
        }
        public async Task<IEnumerable<Reservation>> ReservationHistoryAsync(int clienteId)
        {
            if (clienteId <= 0)
                throw new ArgumentException("User ID must be greater than zero.", nameof(clienteId));

            var reservations = await _reservationRepository.GetUserReservationHistoryAsync(clienteId);

            if (reservations == null || !reservations.Any())
                throw new KeyNotFoundException($"No reservations found for user ID {clienteId}.");

            return reservations;
        }
    }
}
