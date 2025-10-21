//***********************************************************************************
//Program: AcceptFriendRequestDto.cs
//Description: Data transfer object for responding to friend requests
//Date: Oct 6, 2025
//Author: John Nasitem
//***********************************************************************************



using System.ComponentModel.DataAnnotations;

namespace takethetab_server.Application.Dtos.Friends.RepondToRequest
{
    public class RespondToFriendRequestDto
    {
        [Required]
        public long OtherUserId { get; set; }
        [Required]
        public bool AcceptedRequest { get; set; }
    }
}
