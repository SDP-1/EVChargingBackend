public class UpdateBookingDto
{
    public string? StationId { get; set; }   // optional
    public string? SlotId { get; set; }      // optional: new slot id if changing
}
