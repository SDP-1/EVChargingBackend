public class CreateBookingDto
{
    public string StationId { get; set; }  // StationId for the booking
    public string SlotId { get; set; }     // SlotId for the booking
    public string? NIC { get; set; }       // NIC of the EVOwner (only needed if the user is Backoffice)
}
