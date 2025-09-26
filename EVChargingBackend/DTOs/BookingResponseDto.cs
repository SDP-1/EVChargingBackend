namespace EVChargingBackend.DTOs
{
    public class BookingResponseDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }           // Replace EVOwnerNIC with UserId
        public string StationId { get; set; }
        public DateTime ReservationDateTime { get; set; }
        public bool Approved { get; set; }
        public bool Confirmed { get; set; }
        public bool Completed { get; set; }
    }
}
