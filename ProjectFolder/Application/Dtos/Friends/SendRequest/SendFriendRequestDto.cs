//***********************************************************************************
//Program: SendFriendRequestDto.cs
//Description: Data transfer object for sending friend requests
//Date: Oct 6, 2025
//Author: John Nasitem
//***********************************************************************************



using System.ComponentModel.DataAnnotations;

namespace takethetab_server.Application.Dtos.Friends.SendRequest
{
    public class SendFriendRequestDto
    {
        [Required]
        public string OtherUserEmail { get; set; } = null!;
    }
}
