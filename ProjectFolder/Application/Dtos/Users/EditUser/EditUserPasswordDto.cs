//Program: EditUserPasswordDto.cs
//Description: Data transfer object for editting user password
//Date: Oct 6, 2025
//Author: John Nasitem
//***********************************************************************************



using System.ComponentModel.DataAnnotations;

namespace takethetab_server.Application.Dtos.Users.EditUser
{
    public class EditUserPasswordDto
    {
        [Required]
        public string OldPassword { get; set; } = null!;
        [Required]
        public string NewPassword { get; set; } = null!;
    }
}
