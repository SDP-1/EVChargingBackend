namespace EVChargingBackend.DTOs
{
    public class UserUpdateDto
    {
        // NIC is nullable, Backoffice can update it
        public string? NIC { get; set; }

        // Active status is now managed by the explicit Activate/Deactivate methods in AuthController, 
        // but it's kept here if the Backoffice needs to set it during a general edit.
        public bool? Active { get; set; }

        // Message field for Backoffice to send notes/status updates to the user
        public string? Message { get; set; }
    }
}
