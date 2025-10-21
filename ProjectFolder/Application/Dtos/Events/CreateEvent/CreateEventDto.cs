//***********************************************************************************
//Program: CreateEventDto.cs
//Description: Data transfer object for creating events
//Date: Oct 7, 2025
//Author: John Nasitem
//***********************************************************************************



using System.ComponentModel.DataAnnotations;

namespace takethetab_server.Application.Dtos.Events.CreateEvent
{
    public class CreateEventDto
    {
        [Required]
        public string EventName { get; set; } = null!;
        [Required]
        public DateTime EventDate { get; set; }
        [Required]
        public List<long> Participants { get; set; } = null!;
    }
}
