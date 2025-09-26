using System.Security.Claims;
using EVChargingBackend.DTOs;
using EVChargingBackend.Models;
using EVChargingBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVChargingBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // Create reservation, Enforcing token based authentication
        [Authorize(Roles = "Backoffice,EVOwner")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateReservation([FromBody] CreateBookingDto dto)
        {
            var now = DateTime.UtcNow;
            if ((dto.ReservationDateTime - now).TotalDays > 7)
                return BadRequest("Reservation date must be within 7 days.");

            var booking = new Booking
            {
                EVOwnerNIC = dto.EVOwnerNIC,
                StationId = dto.StationId,
                ReservationDateTime = dto.ReservationDateTime
            };

            var newBooking = await _bookingService.CreateReservationAsync(booking);

            var response = new BookingResponseDto
            {
                Id = newBooking.Id.ToString(),
                EVOwnerNIC = newBooking.EVOwnerNIC,
                StationId = newBooking.StationId,
                ReservationDateTime = newBooking.ReservationDateTime,
                Approved = newBooking.Approved,
                Confirmed = newBooking.Confirmed,
                Completed = newBooking.Completed
            };

            return Ok(response);
        }

        // Update reservation
        [Authorize(Roles = "Backoffice,EVOwner")]
        [HttpPut("update/{bookingId}")]
        public async Task<IActionResult> UpdateReservation(string bookingId, [FromBody] UpdateBookingDto dto)
        {
            var booking = await _bookingService.GetReservationByIdAsync(bookingId);
            if (booking == null) return NotFound("Booking not found");

            if ((booking.ReservationDateTime - DateTime.UtcNow).TotalHours < 12)
                return BadRequest("Cannot update less than 12 hours before reservation.");

            // Partial updates: only apply non-null values
            if (!string.IsNullOrEmpty(dto.StationId))
                booking.StationId = dto.StationId;

            if (dto.ReservationDateTime.HasValue)
                booking.ReservationDateTime = dto.ReservationDateTime.Value;

            var updatedBooking = await _bookingService.UpdateReservationAsync(bookingId, booking);

            var response = new BookingResponseDto
            {
                Id = updatedBooking.Id.ToString(),
                EVOwnerNIC = updatedBooking.EVOwnerNIC,
                StationId = updatedBooking.StationId,
                ReservationDateTime = updatedBooking.ReservationDateTime,
                Approved = updatedBooking.Approved,
                Confirmed = updatedBooking.Confirmed,
                Completed = updatedBooking.Completed
            };

            return Ok(response);
        }


        [HttpGet("debug")]
        [Authorize]
        public IActionResult DebugClaims()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value; // 
            var username = User.Identity?.Name;               // already works
            var isAuthenticated = User.Identity?.IsAuthenticated;
            return Ok(new { username, role, isAuthenticated });
        }


        // Cancel reservation
        [Authorize(Roles = "Backoffice,EVOwner")]
        [HttpPost("cancel/{bookingId}")]
        public async Task<IActionResult> CancelReservation(string bookingId)
        {
            var booking = await _bookingService.GetReservationByIdAsync(bookingId);
            if (booking == null) return NotFound("Booking not found");
            if ((booking.ReservationDateTime - DateTime.UtcNow).TotalHours < 12)
                return BadRequest("Cannot cancel less than 12 hours before reservation.");

            var canceled = await _bookingService.CancelReservationAsync(bookingId);

            return Ok(new
            {
                Success = canceled,
                BookingId = bookingId
            });
        }
    }


}
