//***********************************************************************************
//Program: UpdateActivityDto.cs
//Description: Data transfer object for updating an activity
//Date: Oct 7, 2025
//Author: John Nasitem
//***********************************************************************************



using System.ComponentModel.DataAnnotations;
using takethetab_server.Application.Dtos.Activities.CreateActivity;

namespace takethetab_server.Application.Dtos.Activities.UpdateActivity
{
    public class UpdateActivityDto
    {
        [Required]
        public string ActivityName { get; set; } = null!;
        [Required]
        public bool IsGratuityTypePercent { get; set; }
        [Required]
        public decimal GratuityAmount { get; set; }
        [Required]
        public bool AddFivePercentTax { get; set; }
        [Required]
        public List<CreateActivityItem> Items { get; set; } = null!;
    }
}
