//***********************************************************************************
//Program: CreateActivityResponseDto.cs
//Description: Response data transfer object for creating activites
//Date: Oct 9, 2025
//Author: John Nasitem
//***********************************************************************************



namespace takethetab_server.Application.Dtos.Activities.CreateActivity
{
    public class CreateActivityResponseDto : GenericResponseDto
    {
        public long? ActivityId { get; set; }
    }
}
