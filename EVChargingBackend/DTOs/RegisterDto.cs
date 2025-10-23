/****************************************************
 * File Name: RegisterDto.cs
 * Description: User Register DTO .
 * Author: Sehand Devinda
 * Date: 2025-10-05
 ****************************************************/
using System.ComponentModel.DataAnnotations;

namespace EVChargingBackend.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }  // plain password from client

        [Required]
        public string Role { get; set; }      // Backoffice, StationOperator, EVOwner

        public string? NIC { get; set; }
    }
}
