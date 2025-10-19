/****************************************************
 * File Name: UpdateBookingDto.cs
 * Description: Booking Update DTO .
 * Author: Avindi Obeyesekere
 * Date: 2025-09-28
 ****************************************************/
public class UpdateBookingDto
{
    public string? StationId { get; set; }   // optional
    public string? SlotId { get; set; }      // optional: new slot id if changing
}
