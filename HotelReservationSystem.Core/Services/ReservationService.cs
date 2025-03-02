using System;
using System.ComponentModel.DataAnnotations;
using HotelReservationSystem.Infrastructure.Interfaces;
using HotelReservationSystem.Infrastructure.Models;
using HotelReservationSystem.Core.Interfaces;


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

        public async Task CancelReservationAsync(int reservationId)
        {
            var reservation = await _reservationRepository.FindByIdAsync(reservationId);

            if (reservation == null)
            {
                throw new InvalidOperationException("Reservation not found.");
            }

            if (reservation.Status != HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Confirmed)
            {
                throw new InvalidOperationException("Only confirmed reservations can be canceled.");
            }

            if (reservation.StartDate < DateTime.Now.Date)
            {
                throw new InvalidOperationException("Cannot cancel a reservation that has already started or passed.");
            }

            reservation.Status = HotelReservationSystem.Infrastructure.Data.Enum.ReservationStatus.Canceled;

            await _reservationRepository.UpdateAsync(reservation);

            bool hasOtherConfirmedReservation = await _reservationRepository.HasConfirmedReservationsAsync(
                reservation.RoomId,
                reservation.StartDate,
                reservation.EndDate,
                reservation.Id
            );

            if (!hasOtherConfirmedReservation)
            {
                await _roomRepository.UpdateAvailabilityAsync(reservation.RoomId, true);
            }
        }

        public async Task<List<string>> NotifyCheckInAsync()
        {
            DateTime currentDate = DateTime.UtcNow.Date;
            DateTime startRange = currentDate;
            DateTime endRange = currentDate.AddDays(2);

            var reservationsInRange = await _reservationRepository.FindReservationsByStartDateRangeAsync(startRange, endRange);

            var reservationsToNotify = reservationsInRange.Where(r => !r.IsNotified).ToList();
            var alreadyNotifiedReservations = reservationsInRange.Where(r => r.IsNotified).ToList();

            if (reservationsToNotify.Count == 0 && alreadyNotifiedReservations.Count > 0)
            {
                var alreadyNotifiedIds = string.Join(", ", alreadyNotifiedReservations.Select(r => r.Id));
                return new List<string> { $"All reservations in the date range ({startRange:dd/MM/yyyy} to {endRange:dd/MM/yyyy}) have already been notified. Reservation IDs: {alreadyNotifiedIds}" };
            }

            List<string> notifications = new List<string>();
            foreach (var reservation in reservationsToNotify)
            {
                string message = $"Notification: Dear Client {reservation.ClientId}, your reservation (ID: {reservation.Id}) check-in is on {reservation.StartDate.ToUniversalTime().ToString("dd/MM/yyyy")}. We look forward to welcoming you!";
                notifications.Add(message);

                reservation.IsNotified = true;
                await _reservationRepository.UpdateAsync(reservation);
            }

            if (notifications.Count == 0 && alreadyNotifiedReservations.Count == 0)
            {
                return new List<string> { $"No confirmed reservations found in the date range ({startRange:dd/MM/yyyy} to {endRange:dd/MM/yyyy})." };
            }

            return notifications;
        }
    }
}

