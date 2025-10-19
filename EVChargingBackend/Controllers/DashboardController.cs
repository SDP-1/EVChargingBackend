/****************************************************
 * File Name: DashboardController.cs
 * Description: Defining Endpoint and Role authentication for Dashboard summary endpoints.
 * Author: Sehan Devinda
 * Last Changes Date: 2025-10-07
 ****************************************************/
using EVChargingBackend.DTOs;
using EVChargingBackend.Models;
using EVChargingBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EVChargingBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IUserService _userService;
        private readonly IChargingStationService _stationService;
        private readonly IChargingSlotService _slotService;

        public DashboardController(
            IBookingService bookingService,
            IUserService userService,
            IChargingStationService stationService,
            IChargingSlotService slotService)
        {
            _bookingService = bookingService;
            _userService = userService;
            _stationService = stationService;
            _slotService = slotService;
        }

        [HttpGet("stats")]
        [ProducesResponseType(200, Type = typeof(DashboardDto))]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetDashboardStats()
        {
            // 🎯 FIX 1: Explicitly search for the custom JWT claim key "userId"
            var userIdClaim = User.FindFirst("userId")?.Value;

            // 🎯 FIX 2: Check for standard ClaimTypes.Role first, then look for a simple "role"
            // (The role claim is typically mapped correctly, but this adds robustness)
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;

            if (userIdClaim == null || roleClaim == null)
            {
                // This will no longer be hit if the token is valid and structured as expected
                return Unauthorized(new { Message = "User identity or role not found in token." });
            }

            var dashboard = new DashboardDto { Role = roleClaim };
            var today = DateTime.Now.Date;

            // --- Common/Base Metrics (All Roles) ---

            // Get upcoming bookings for the current user/station
            if (roleClaim == "EVOwner")
            {
                dashboard.UpcomingBookings = await _bookingService.GetUpcomingBookingsAsync(userId: userIdClaim, limit: 5);
                dashboard.UpcomingBookingsCount = dashboard.UpcomingBookings.Count;

                // Fetch total completed charges for EVOwner
                var allBookings = await _bookingService.GetBookingsByUserIdAsync(userIdClaim);
                dashboard.TotalBookings = allBookings.Count;
                dashboard.TotalCompletedCharges = allBookings.Count(b => b.Completed && !b.Canceled);
            }

            // --- Backoffice/Admin Metrics ---
            else if (roleClaim == "Backoffice") // Use else if for efficiency
            {
                var allBookings = await _bookingService.GetAllBookingsAsync();
                dashboard.TotalBookings = allBookings.Count;
                dashboard.TotalStations = (await _stationService.GetAllStationsAsync()).Count;

                // Counts
                dashboard.PendingUserApprovals = await _userService.GetPendingUserApprovalCountAsync("EVOwner");

                // Trend Chart
                dashboard.BookingTrend = await _bookingService.GetBookingTrendAsync(days: 7);
            }

            // --- Station Operator Metrics ---
            //else if (roleClaim == "StationOperator") // Use else if for efficiency
            //{
            //    // 🎯 FIX 3: Fetch the operator's assigned station ID using the UserService
            //    // NOTE: This assumes your IUserService has a method like GetStationIdByOperatorIdAsync
            //    string operatorStationId = await _userService.GetStationIdByOperatorIdAsync(userIdClaim);

            //    if (!string.IsNullOrEmpty(operatorStationId))
            //    {
            //        // Bookings for this station
            //        dashboard.UpcomingBookings = await _bookingService.GetUpcomingBookingsAsync(stationId: operatorStationId, limit: 5);
            //        dashboard.UpcomingBookingsCount = dashboard.UpcomingBookings.Count;

            //        // Slot status for today
            //        var allSlots = await _slotService.GetAllSlotsAsync(operatorStationId, today);
            //        dashboard.BookedSlotsToday = allSlots.Count(s => s.IsBooked);
            //        dashboard.AvailableSlotsToday = allSlots.Count(s => !s.IsBooked);
            //        dashboard.TotalSlots = allSlots.Count;
            //    }
            //    else
            //    {
            //        // Handle case where operator is logged in but not assigned a station
            //        dashboard.TotalSlots = 0;
            //        dashboard.AvailableSlotsToday = 0;
            //        dashboard.BookedSlotsToday = 0;
            //    }
            //}

            return Ok(dashboard);
        }
    }
}