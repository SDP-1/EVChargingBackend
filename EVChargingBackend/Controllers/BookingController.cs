using System.Security.Claims;
using EVChargingBackend.DTOs;
using EVChargingBackend.Models;
using EVChargingBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.IO;


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

            // ← Add this to define userId
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("UserId not found in token.");

            var booking = new Booking
            {
                UserId = userId,
                StationId = dto.StationId,
                ReservationDateTime = dto.ReservationDateTime
            };

            var newBooking = await _bookingService.CreateReservationAsync(booking);

            var response = new BookingResponseDto
            {
                Id = newBooking.Id.ToString(),
                UserId = newBooking.UserId,
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
                UserId = updatedBooking.UserId,
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

        //Confirm Booking
        [Authorize(Roles = "StationOperator")]
        [HttpPost("confirm/{bookingId}")]
        public async Task<IActionResult> ConfirmBooking(string bookingId)
        {
            var username = User.Identity?.Name; // StationOperator username
            var booking = await _bookingService.ConfirmBookingAsync(bookingId, username);
            return Ok(new
            {
                BookingId = booking.Id.ToString(),
                UserId = booking.UserId,
                StationId = booking.StationId,
                ReservationDateTime = booking.ReservationDateTime,
                Approved = booking.Approved,
                Confirmed = booking.Confirmed,
                Completed = booking.Completed
            });
        }


        //Complete Booking
        [Authorize(Roles = "StationOperator")]
        [HttpPost("complete/{bookingId}")]
        public async Task<IActionResult> CompleteBooking(string bookingId)
        {
            var username = User.Identity?.Name; // StationOperator username
            var booking = await _bookingService.CompleteBookingAsync(bookingId, username);
            return Ok(new
            {
                BookingId = booking.Id.ToString(),
                UserId = booking.UserId,
                StationId = booking.StationId,
                ReservationDateTime = booking.ReservationDateTime,
                Approved = booking.Approved,
                Confirmed = booking.Confirmed,
                Completed = booking.Completed
            });
        }


        // Gen QR code endpoint
        [Authorize(Roles = "EVOwner")]
        [HttpGet("qrcode/{bookingId}")]
        public async Task<IActionResult> GetBookingQrCode(string bookingId)
        {
            var booking = await _bookingService.GetReservationByIdAsync(bookingId);
            if (booking == null) return NotFound("Booking not found");
            if (!booking.Approved) return BadRequest("Booking is not approved yet.");

            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(booking.Id.ToString(), QRCodeGenerator.ECCLevel.Q);

            // Generate PNG bytes directly
            var qrCodePng = new PngByteQRCode(qrData);
            byte[] qrBytes = qrCodePng.GetGraphic(20);

            string qrBase64 = Convert.ToBase64String(qrBytes);

            return Ok(new { BookingId = booking.Id.ToString(), QrCodeBase64 = qrBase64 });
        }

        //get booking by Id
        [Authorize(Roles = "StationOperator,Backoffice,EVOwner")]
        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetBookingById(string bookingId)
        {
            try
            {
                var booking = await _bookingService.GetReservationByIdAsync(bookingId);
                if (booking == null) return NotFound("Booking not found");

                // StationOperator restriction
                if (User.IsInRole("StationOperator") && !booking.Confirmed)
                {
                    return StatusCode(403, new { message = "Booking not confirmed, First Confirm Booking" });
                }

                return Ok(new
                {
                    BookingId = booking.Id.ToString(),
                    UserId = booking.UserId,
                    StationId = booking.StationId,
                    ReservationDateTime = booking.ReservationDateTime,
                    Approved = booking.Approved,
                    Confirmed = booking.Confirmed,
                    Completed = booking.Completed
                });
            }
            catch
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }



        // Get all bookings for EVOwner (userId from JWT token)
        [Authorize(Roles = "EVOwner")]
        [HttpGet("mybookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return BadRequest("UserId not found in token.");

            var bookings = await _bookingService.GetBookingsByUserIdAsync(userId);
            return Ok(bookings);
        }

        // Backoffice: Get all bookings or bookings for a specific user
        [Authorize(Roles = "Backoffice")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllBookings([FromQuery] string userId = null)
        {
            List<Booking> bookings;
            if (string.IsNullOrEmpty(userId))
            {
                bookings = await _bookingService.GetAllBookingsAsync();
            }
            else
            {
                bookings = await _bookingService.GetBookingsByUserIdAsync(userId);
            }
            return Ok(bookings);
        }


    }


}
