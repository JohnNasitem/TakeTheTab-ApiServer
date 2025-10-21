//***********************************************************************************
//Program: FetchUserResponseDto.cs
//Description: Response data transfer object for fetching users
//Date: Oct 6, 2025
//Author: John Nasitem
//***********************************************************************************

namespace takethetab_server.Application.Dtos.Users.FetchUser
{
    public class FetchUserResponseDto : GenericResponseDto
    {
        public long? UserId { get; set; }
        public string? UserDisplayName { get; set; }
        public string? UserEmail { get; set; }
        public Dictionary<long, FetchUserResponseEvent>? Events { get; set; }
        public decimal? UserTotalOwed { get; set; }
        public decimal? UserTotalOwing { get; set; }
    }



    public class FetchUserResponseEvent
    {
        public string EventName { get; set; } = null!;
        public decimal UserTotalOwed { get; set; }
        public decimal UserTotalOwing { get; set; }
    }
}
