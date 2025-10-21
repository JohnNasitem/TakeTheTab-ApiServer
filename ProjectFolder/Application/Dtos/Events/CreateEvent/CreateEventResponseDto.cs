//***********************************************************************************
//Program: CreateEventResponseDto.cs
//Description: Response data transfer object for creating events
//Date: Oct 9, 2025
//Author: John Nasitem
//***********************************************************************************



namespace takethetab_server.Application.Dtos.Events.CreateEvent
{
    public class CreateEventResponseDto : GenericResponseDto
    {
        public long? EventId { get; set; }
    }
}
