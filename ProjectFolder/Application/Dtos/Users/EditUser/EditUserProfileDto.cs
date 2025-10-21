//Program: EditUserProfileDto.cs
//Description: Data transfer object for editting user profile
//Date: Oct 6, 2025
//Author: John Nasitem
//***********************************************************************************



using System.ComponentModel.DataAnnotations;

namespace takethetab_server.Application.Dtos.Users.EditUser
{
    public class EditUserProfileDto
    {
        [Required]
        public string DisplayName { get; set; } = null!;
        [Required]
        public string Email { get; set; } = null!;
    }
}
