/****************************************************
 * File Name: DashboardDto.cs
 * Description: User Update DTO .
 * Author: Sehan Devinda
 * Date: 2025-10-01
 ****************************************************/
namespace EVChargingBackend.DTOs
{
    public class DashboardDto
    {
        public string Role { get; set; }

        // Common
        public int TotalBookings { get; set; }
        public int UpcomingBookingsCount { get; set; }
        public List<Booking> UpcomingBookings { get; set; } = new();

        // Backoffice/Admin Specific
        public long PendingUserApprovals { get; set; }
        public long TotalStations { get; set; }
        public Dictionary<DateTime, long> BookingTrend { get; set; } = new();

        // Station Operator Specific
        public int AvailableSlotsToday { get; set; }
        public int BookedSlotsToday { get; set; }

        // EV Owner Specific
        public int TotalCompletedCharges { get; set; }
    }
}
