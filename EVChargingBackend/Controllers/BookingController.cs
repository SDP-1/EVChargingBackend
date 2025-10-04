using System.Security.Claims;
using AutoMapper;
using EVChargingBackend.DTOs;
using EVChargingBackend.Models;
using EVChargingBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
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
        private readonly IMapper _mapper;
        private readonly IChargingSlotService _slotService;

        public BookingController(IBookingService bookingService, IChargingSlotService slotService, IMapper mapper)
        {
            _bookingService = bookingService;
            _slotService = slotService;
            _mapper = mapper;
        }


        // Create reservation, Enforcing token based authentication
        [Authorize(Roles = "Backoffice,EVOwner")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateReservation([FromBody] CreateBookingDto dto)
        {
            // Get the slot details
            var slot = await _slotService.GetSlotByIdAsync(dto.SlotId);
            if (slot == null)
                return NotFound("Selected slot not found.");

            // Check if slot is already booked
            if (slot.IsBooked)
                return BadRequest("Selected slot is already booked.");

            // Validation: reservation must be within 7 days from now
            var now = DateTime.UtcNow;
            if ((slot.StartTime - now).TotalDays > 7)
                return BadRequest("Reservation date must be within 7 days.");

            // Extract userId from JWT
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("UserId not found in token.");

            // Create the booking using slot info
            var booking = new Booking
            {
                UserId = userId,
                StationId = slot.StationId.ToString(),
                SlotId = dto.SlotId,
                ReservationDateTime = slot.StartTime
            };

            var newBooking = await _bookingService.CreateReservationAsync(booking);

            // Mark the slot as booked
            await _slotService.BookSlotAsync(dto.SlotId, userId, newBooking.Id);

            var response = _mapper.Map<BookingResponseDto>(newBooking);
            return Ok(response);
        }




        // Update reservation
        [Authorize(Roles = "Backoffice,EVOwner")]
        [HttpPut("update/{bookingId}")]
        public async Task<IActionResult> UpdateReservation(string bookingId, [FromBody] UpdateBookingDto dto)
        {
            var booking = await _bookingService.GetReservationByIdAsync(bookingId);
            if (booking == null) return NotFound("Booking not found");

            // Restriction: cannot update less than 12 hours before reservation
            if ((booking.ReservationDateTime - DateTime.UtcNow).TotalHours < 12)
                return BadRequest("Cannot update less than 12 hours before reservation.");

            // Partial updates: apply only non-null values
            if (!string.IsNullOrEmpty(dto.SlotId))
            {
                // Fetch new slot info
                var slot = await _slotService.GetSlotByIdAsync(dto.SlotId);
                if (slot == null)
                    return NotFound("Selected slot not found.");

                if (slot.IsBooked)
                    return BadRequest("Selected slot is already booked.");

                // Free the old slot
                if (!string.IsNullOrEmpty(booking.SlotId))
                    await _slotService.FreeSlotAsync(booking.SlotId);

                // Assign new slot
                booking.SlotId = dto.SlotId;
                booking.StationId = slot.StationId.ToString();
                booking.ReservationDateTime = slot.StartTime;

                // Mark the new slot as booked
                var userId = User.FindFirst("userId")?.Value;
                await _slotService.BookSlotAsync(dto.SlotId, userId, booking.Id);
            }

            var updatedBooking = await _bookingService.UpdateReservationAsync(bookingId, booking);
            var response = _mapper.Map<BookingResponseDto>(updatedBooking);
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

            // Cannot cancel less than 12 hours before the reservation
            if ((booking.ReservationDateTime - DateTime.UtcNow).TotalHours < 12)
                return BadRequest("Cannot cancel less than 12 hours before reservation.");

            // Optionally, free up the slot
            if (!string.IsNullOrEmpty(booking.SlotId))
                await _slotService.FreeSlotAsync(booking.SlotId);

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

            var response = _mapper.Map<BookingResponseDto>(booking);

            return Ok(response);
        }


        //Complete Booking
        [Authorize(Roles = "StationOperator")]
        [HttpPost("complete/{bookingId}")]
        public async Task<IActionResult> CompleteBooking(string bookingId)
        {
            var username = User.Identity?.Name; // StationOperator username
            var booking = await _bookingService.CompleteBookingAsync(bookingId, username);

            var response = _mapper.Map<BookingResponseDto>(booking);

            return Ok(response);
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

                var response = _mapper.Map<BookingResponseDto>(booking);

                return Ok(response);
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
