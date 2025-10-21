//***********************************************************************************
//Program: FetchActivityResponseDto.cs
//Description: Response data transfer object for fetching an activity
//Date: Oct 7, 2025
//Author: John Nasitem
//***********************************************************************************



namespace takethetab_server.Application.Dtos.Activities.FetchActivity
{
    public class FetchActivityResponseDto : GenericResponseDto
    {
        public bool? CreatedActivity { get; set; }
        public string? ActivityName { get; set; }
        public bool? IsPayee { get; set; }
        public decimal? Amount { get; set; }
        public bool? AddFivePercentTax { get; set; }
        public bool? IsGratuityTypePercent { get; set; }
        public decimal GratuityAmount { get; set; }
        public decimal ActivitySubtotal { get; set; }
        public FetchActivityResponsePayee? Payee { get; set; }
        public List<FetchActivityResponseItem>? Items { get; set; }
        public List<FetchActivityResponsePayer>? Payers { get; set; }
    }



    public class FetchActivityResponsePayee
    {
        public string PayeeName { get; set; } = null!;
        public string PayerEmail { get; set; } = null!;
        public string? PayerPhoneNumber { get; set; }
    }



    public class FetchActivityResponseItem
    {
        public long ItemId { get; set; }
        public string ItemName { get; set; } = null!;
        public decimal ItemCost { get; set; }
        public bool IsSplitEvently { get; set; }
        public List<FetchActivityResponsePayer> Payers { get; set; } = null!;
    }



    public class FetchActivityResponsePayer
    {
        public long PayerId { get; set; }
        public string PayerName { get; set; } = null!;
        public string PayerEmail { get; set; } = null!;
        public decimal AmountOwing { get; set; }
    }
}
