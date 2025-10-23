/****************************************************
 * File Name: ChargingStationUpdateDto.cs
 * Description: ChargingStationUpdate DTO .
 * Author: Avindi Obeyesekere
 * Date: 2025-09-28
 ****************************************************/
using System.Collections.Generic;
using EVChargingBackend.Models;

namespace EVChargingBackend.DTOs
{
    public class ChargingStationUpdateDto
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public GeoLocation? GeoLocation { get; set; }
        public string? Type { get; set; }
        public bool? Active { get; set; }
        public int? NumberOfConnectors { get; set; }
        public List<string>? ConnectorTypes { get; set; }
        public string? OperatingHours { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public bool? IsPublic { get; set; }
        public List<string>? Amenities { get; set; }
    }
}