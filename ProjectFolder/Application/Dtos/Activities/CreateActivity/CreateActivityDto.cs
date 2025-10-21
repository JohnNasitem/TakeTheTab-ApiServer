//***********************************************************************************
//Program: CreateActivityDto.cs
//Description: Data transfer object for creating activities
//Date: Oct 7, 2025
//Author: John Nasitem
//***********************************************************************************



using System.ComponentModel.DataAnnotations;

namespace takethetab_server.Application.Dtos.Activities.CreateActivity
{
    public class CreateActivityDto
    {
        [Required]
        public long EventId { get; set; }
        [Required]
        public string ActivityName { get; set; } = null!;
        [Required]
        public bool IsGratuityTypePercent { get; set; }
        [Required]
        public decimal GratuityAmount { get; set; }
        [Required]
        public bool IncludeTax { get; set; }
        [Required]
        public List<CreateActivityItem> Items { get; set; } = null!;
    }

    public class CreateActivityItem
    {
        public long? ItemId { get; set; }
        [Required]
        public string ItemName { get; set; } = null!;
        [Required]
        public decimal ItemCost { get; set; }
        [Required]
        public bool IsSplitTypeEvenly { get; set; }
        [Required]
        public Dictionary<long, decimal> Payers { get; set; } = null!;
    }
}
