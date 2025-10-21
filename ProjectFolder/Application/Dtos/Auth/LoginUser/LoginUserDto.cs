//***********************************************************************************
//Program: LoginUserDto.cs
//Description: Data transfer object for logging in users
//Date: Sep 29, 2025
//Author: John Nasitem
//***********************************************************************************



using System.ComponentModel.DataAnnotations;

namespace takethetab_server.Application.Dtos.Auth.LoginUser
{
    public class LoginUserDto
    {
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        [Required]
        public string BrowserId { get; set; } = null!;
    }
}
