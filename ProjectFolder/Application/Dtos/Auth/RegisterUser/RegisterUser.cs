//***********************************************************************************
//Program: RegisterUser.cs
//Description: Data transfer object for registering users
//Date: Sep 28, 2025
//Author: John Nasitem
//***********************************************************************************



using System.ComponentModel.DataAnnotations;

namespace takethetab_server.Application.Dtos.Auth.RegisterUser
{
    public class RegisterUser
    {
        [Required]
        public string DisplayName { get; set; } = null!;
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        public string? PhoneNumber { get; set; }
    }
}
