/****************************************************
 * File Name: BookingController.cs
 * Description: Defining Endpoint and Role authentication for Bookings .
 * Author: Avindi Obeyesekere
 * Last Changes Date: 2025-10-09
 ****************************************************/
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
        private readonly IEVOwnerService _eVOwnerService;

        public BookingController(IBookingService bookingService, IChargingSlotService slotService, IEVOwnerService eVOwnerService,IMapper mapper)
        {
            _bookingService = bookingService;
            _slotService = slotService;
            _eVOwnerService = eVOwnerService;
            _mapper = mapper;
        }


        // Create reservation, Enforcing token based authentication
        [Authorize(Roles = "Backoffice,EVOwner")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateReservation([FromBody] CreateBookingDto dto)
        {
            if (dto == null)
                return BadRequest("Booking data is missing.");

            // Get the slot details
            var slot = await _slotService.GetSlotByIdAsync(dto.SlotId);
            if (slot == null)
                return NotFound("Selected slot not found.");

            // Check if slot is already booked
            if (slot.IsBooked)
                return BadRequest("Selected slot is already booked.");

            // Validation: reservation must be within 7 days from now
            var now = DateTime.Now;
            if ((slot.StartTime - now).TotalDays > 7)
                return BadRequest("Reservation date must be within 7 days.");

            // If the user is a backoffice user, they must provide the NIC of the EVOwner
            string evOwnerUserId = null;
            if (User.IsInRole("Backoffice"))
            {
                if (string.IsNullOrEmpty(dto.NIC))  // Check if NIC is provided by Backoffice
                    return BadRequest("NIC is required for EVOwner when creating a booking from Backoffice.");

                evOwnerUserId = await _eVOwnerService.GetUserIdByNICAsync(dto.NIC);  // Use the new method to fetch the userId by NIC
                if (string.IsNullOrEmpty(evOwnerUserId))
                    return NotFound("EVOwner not found.");
            }
            else
            {
                // If the logged-in user is an EVOwner, the booking should be created for their own userId
                evOwnerUserId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(evOwnerUserId))
                    return Unauthorized("UserId not found in token.");
            }

            // Create the booking using slot info and the selected EVOwner's userId
            var booking = new Booking
            {
                UserId = evOwnerUserId,  // Assign the EVOwner's userId here, not the backoffice's
                StationId = slot.StationId.ToString(),
                SlotId = dto.SlotId,
                ReservationDateTime = slot.StartTime
            };

            var newBooking = await _bookingService.CreateReservationAsync(booking);

            // Mark the slot as booked
            await _slotService.BookSlotAsync(dto.SlotId, evOwnerUserId, newBooking.Id);

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
            if ((booking.ReservationDateTime - DateTime.Now).TotalHours < 12)
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
            if ((booking.ReservationDateTime - DateTime.Now).TotalHours < 12)
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
        [Authorize(Roles = "StationOperator,Backoffice")]
        [HttpPost("confirm/{bookingId}")]
        public async Task<IActionResult> ConfirmBooking(string bookingId)
        {
            var username = User.Identity?.Name; // StationOperator username
            var booking = await _bookingService.ConfirmBookingAsync(bookingId, username);

            var response = _mapper.Map<BookingResponseDto>(booking);

            return Ok(response);
        }


        //Complete Booking
        [Authorize(Roles = "StationOperator,Backoffice")]
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


        // Reopen a canceled booking and re-book the associated slot if available
        [Authorize(Roles = "Backoffice,EVOwner")]
        [HttpPost("reopen/{bookingId}")]
        public async Task<IActionResult> ReopenReservation(string bookingId)
        {
            try
            {
                var booking = await _bookingService.GetReservationByIdAsync(bookingId);
                if (booking == null) return NotFound("Booking not found");

                if (!booking.Canceled) return BadRequest("Booking is not canceled.");

                // Ensure reopen is allowed (in service we also validate timing)
                var reopened = await _bookingService.ReopenReservationAsync(bookingId);

                // If booking had a SlotId, attempt to re-book that slot
                if (!string.IsNullOrEmpty(reopened.SlotId))
                {
                    var slot = await _slotService.GetSlotByIdAsync(reopened.SlotId);
                    if (slot == null)
                    {
                        // Slot no longer exists; return reopened booking but with warning
                        var responseWarn = _mapper.Map<BookingResponseDto>(reopened);
                        return Ok(new { booking = responseWarn, warning = "Associated slot no longer exists." });
                    }

                    if (slot.IsBooked)
                    {
                        // If slot is already booked, we cannot reassign. Return reopened booking with info.
                        var responseWarn = _mapper.Map<BookingResponseDto>(reopened);
                        return Ok(new { booking = responseWarn, warning = "Associated slot is already booked by someone else." });
                    }

                    // Attempt to book the slot again for the same user
                    var userId = reopened.UserId;
                    var booked = await _slotService.BookSlotAsync(reopened.SlotId, userId, reopened.Id);
                    if (!booked)
                    {
                        var responseWarn = _mapper.Map<BookingResponseDto>(reopened);
                        return Ok(new { booking = responseWarn, warning = "Failed to book the associated slot." });
                    }
                }

                var response = _mapper.Map<BookingResponseDto>(reopened);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Booking not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // Approve a booking (Backoffice)
        [Authorize(Roles = "Backoffice")]
        [HttpPost("approve/{bookingId}")]
        public async Task<IActionResult> ApproveReservation(string bookingId)
        {
            try
            {
                var booking = await _bookingService.GetReservationByIdAsync(bookingId);
                if (booking == null) return NotFound("Booking not found");

                if (booking.Canceled) return BadRequest("Cannot approve a canceled booking.");

                var approved = await _bookingService.ApproveReservationAsync(bookingId);

                // Optionally, you could auto-book the slot here if needed (slot booking is done at create time)
                var response = _mapper.Map<BookingResponseDto>(approved);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Booking not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

    }


}
