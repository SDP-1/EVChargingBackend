public class UserResponseDto
{
    public string Id { get; set; }   // string instead of ObjectId
    public string Username { get; set; }
    public string Role { get; set; }
    public string NIC { get; set; }
    public bool Active { get; set; }
}
