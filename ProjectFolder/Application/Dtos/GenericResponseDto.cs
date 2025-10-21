//***********************************************************************************
//Program: GenericResponseDto.cs
//Description: Generic response data transfer object
//Date: Oct 6, 2025
//Author: John Nasitem
//***********************************************************************************



namespace takethetab_server.Application.Dtos
{
    public class GenericResponseDto
    {
        public bool ActionSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
