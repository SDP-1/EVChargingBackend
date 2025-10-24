namespace EVChargingBackend.DTOs
{
    public class ChargingStationResponseDto
    {
        public string? Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public object? GeoLocation { get; set; }
        public string Type { get; set; }
        public bool Active { get; set; }  // Always boolean in the response
    }
}
