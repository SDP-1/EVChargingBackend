/****************************************************
 * File Name: UserUpdateDto.cs
 * Description: USer Update DTO .
 * Author: Avindi Obeyesekere
 * Date: 2025-09-28
 ****************************************************/
public class UserResponseDto
{
    public string Id { get; set; }   // string instead of ObjectId
    public string Username { get; set; }
    public string Role { get; set; }
    public string NIC { get; set; }
    public bool Active { get; set; }
    public string Message { get; set; }
}
