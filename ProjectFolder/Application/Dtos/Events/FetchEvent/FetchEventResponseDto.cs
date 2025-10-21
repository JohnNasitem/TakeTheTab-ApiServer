//***********************************************************************************
//Program: FetchEventResponseDto.cs
//Description: Response data transfer object for fetching an event
//Date: Oct 7, 2025
//Author: John Nasitem
//***********************************************************************************



namespace takethetab_server.Application.Dtos.Events.FetchEvent
{
    public class FetchEventResponseDto : GenericResponseDto
    {
        public bool? CreatedEvent { get; set; }
        public string? EventName { get; set; }
        public DateTime? EventDate { get; set; }
        public Dictionary<long, FetchEventResponseActivity>? Activities { get; set; }
        public List<FetchEventResponseParticipant>? Participants { get; set; }
        public List<long>? ActiveParticipants { get; set; }
        public decimal? UserTotalOwed { get; set; }
        public decimal? UserTotalOwing { get; set; }
    }



    public class FetchEventResponseParticipant
    {
        public long UserId { get; set; }
        public string DisplayName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool HasPaid { get; set; }
        public decimal AmountOwedToYou { get; set; }
        public bool PaymentConfirmed { get; set; }
    }



    public class FetchEventResponseActivity
    {
        public string ActivityName { get; set; } = null!;
        public bool OwedMoney { get; set; }
        public decimal Amount { get; set; }
    }
}
