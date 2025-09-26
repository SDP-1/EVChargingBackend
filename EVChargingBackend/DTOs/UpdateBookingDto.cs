namespace EVChargingBackend.DTOs
{
    public class UpdateBookingDto
    {
        public string? StationId { get; set; }//? = nullable
        public DateTime? ReservationDateTime { get; set; } // nullable
    }

}
