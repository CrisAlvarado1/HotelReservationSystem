﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public async Task<IEnumerable<Reservation>> ReservationHistoryAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero.", nameof(userId));

            var reservations = await _reservationRepository.GetUserReservationHistoryAsync(userId);

            if (reservations == null || !reservations.Any())
                throw new KeyNotFoundException($"No reservations found for user ID {userId}.");

            return reservations;
        }
    }
}

