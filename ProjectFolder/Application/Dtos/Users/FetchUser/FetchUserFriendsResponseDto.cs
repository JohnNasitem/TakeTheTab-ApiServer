//***********************************************************************************
//Program: FetchUserFriendsResponseDto.cs
//Description: Response data transfer object for fetching user's friends
//Date: Oct 6, 2025
//Author: John Nasitem
//***********************************************************************************

namespace takethetab_server.Application.Dtos.Users.FetchUser
{
    public class FetchUserFriendsResponseDto : GenericResponseDto
    {
        public Dictionary<long, string[]>? Friends { get; set; }
        public Dictionary<long, string[]>? IncommingFriendRequests { get; set; }
    }
}
