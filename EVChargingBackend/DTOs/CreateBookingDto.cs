namespace EVChargingBackend.DTOs
{
    public class CreateBookingDto
    {
        public string EVOwnerNIC { get; set; }      // EV Owner
        public string StationId { get; set; }       // Station ID
        public DateTime ReservationDateTime { get; set; }
    }

}
