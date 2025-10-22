/****************************************************
 * File Name: IBookingService.cs
 * Description: Interface for Booking Service.
 * Author: Avindi Obeyesekere
 * Date: 2025-09-26
 ****************************************************/
using EVChargingBackend.Models;

namespace EVChargingBackend.Services
{
    public interface IBookingService
    {
        Task<Booking> CreateReservationAsync(Booking booking);
        Task<Booking> UpdateReservationAsync(string bookingId, Booking updatedBooking);
        Task<bool> CancelReservationAsync(string bookingId);
        Task<Booking> GetReservationByIdAsync(string bookingId);
        Task<Booking> ConfirmBookingAsync(string bookingId, string stationOperatorUsername);
        Task<Booking> CompleteBookingAsync(string bookingId, string stationOperatorUsername);
        Task<List<Booking>> GetBookingsByUserIdAsync(string userId);
        Task<List<Booking>> GetAllBookingsAsync();
        Task<Dictionary<DateTime, long>> GetBookingTrendAsync(int days = 7);
        Task<List<Booking>> GetUpcomingBookingsAsync(string userId = null, string stationId = null, int limit = 5);
        Task<Booking> ReopenReservationAsync(string bookingId);
        Task<Booking> ApproveReservationAsync(string bookingId);
    }
}
