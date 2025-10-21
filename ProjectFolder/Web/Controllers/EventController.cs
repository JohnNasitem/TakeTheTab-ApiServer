//***********************************************************************************
//Program: EventController.cs
//Description: Handles event endpoints
//Date: Sep 25, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.AspNetCore.Mvc;
using takethetab_server.Application;
using takethetab_server.Application.Dtos;
using takethetab_server.Application.Dtos.Events.CreateEvent;
using takethetab_server.Application.Dtos.Events.FetchEvent;
using takethetab_server.Application.Dtos.Events.UpdateEvent;
using takethetab_server.Domain.Entities;
using takethetab_server.Web.Services;

namespace takethetab_server.Web.Controllers
{
    [ApiController]
    [Route("events")]
    public class EventController(UserService userService, EventService eventService, AuthService authService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            // Return bad request if any of the required fields are left empty
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            GenericResponseDto? userValidationResponse = authService.ValidateJwtTokenUser(User, out User user);

            if (userValidationResponse != null)
                return Ok(userValidationResponse);

            try
            {
                Dictionary<long, User> userDict = userService.Users.ToDictionary(u => u.Id, u => u);
                long? eventId = await eventService.CreateEvent(
                    dto.EventName,
                    dto.EventDate,
                    user,
                    [.. dto.Participants.Select(uId => userDict[uId])]
                );

                return Ok(new CreateEventResponseDto()
                {
                    ActionSuccess = eventId != null,
                    ErrorMessage = eventId != null ? null : "Server Error: Failed to create event",
                    EventId = eventId
                });
            }
            catch
            {
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "One or more participants are not valid users"
                });
            }
        }



        [HttpGet("{eventId:long}")]
        public IActionResult FetchEvent(long eventId)
        {
            // Return bad request if any of the required fields are left empty
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            GenericResponseDto? validationResponse = authService.ValidateJwtAndUserEventRelations(User, eventId, false, out User user, out Event validEvent);

            if (validationResponse != null)
                return Ok(validationResponse);

            decimal amountOwedByEventCreator = Math.Round(validEvent.GetNetAmountBetweenUsers(user.Id, validEvent.Creator.Id), 2);

            return Ok(new FetchEventResponseDto()
            {
                ActionSuccess = true,
                ErrorMessage = null,
                CreatedEvent = validEvent.Creator.Id == user.Id,
                EventName = validEvent.Name,
                EventDate = validEvent.Date,
                Activities = validEvent!.Activities.ToDictionary(a => a.Id, a => new FetchEventResponseActivity()
                {
                    ActivityName = a.Name,
                    OwedMoney = a.Payee.Id == user.Id,
                    Amount = a.Payee.Id == user.Id ? Math.Round(a.TotalAmountOwed(user.Id), 2) : Math.Round(a.TotalAmountOwing(user.Id), 2)
                }),
                Participants = validEvent!.Participants.Select(p =>
                {
                    decimal amountOwed = Math.Round(validEvent.GetNetAmountBetweenUsers(user.Id, p.Id), 2);

                    return new FetchEventResponseParticipant()
                    {
                        UserId = p.Id,
                        DisplayName = p.DisplayName,
                        Email = p.Email,
                        HasPaid = amountOwed > 0 ? validEvent.HasPayerSettledDebt(user.Id, p.Id) : validEvent.HasPayerSettledDebt(p.Id, user.Id),
                        AmountOwedToYou = amountOwed,
                        PaymentConfirmed = amountOwed > 0 ? validEvent.HasCreditorConfirmedPayments(user.Id, p.Id) : validEvent.HasCreditorConfirmedPayments(p.Id, user.Id)
                    };
                }).Append(new FetchEventResponseParticipant()
                {
                    UserId = validEvent.Creator.Id,
                    DisplayName = validEvent.Creator.DisplayName,
                    Email = validEvent.Creator.Email,
                    HasPaid = amountOwedByEventCreator > 0 ? validEvent.HasPayerSettledDebt(user.Id, validEvent.Creator.Id) : validEvent.HasPayerSettledDebt(validEvent.Creator.Id, user.Id),
                    AmountOwedToYou = amountOwedByEventCreator,
                    PaymentConfirmed = amountOwedByEventCreator > 0 ? validEvent.HasCreditorConfirmedPayments(user.Id, validEvent.Creator.Id) : validEvent.HasCreditorConfirmedPayments(validEvent.Creator.Id, user.Id)
                }).ToList(),
                ActiveParticipants = validEvent.GetActiveParticipants(),
                UserTotalOwed = Math.Round(validEvent.GetUserTotalOwed(user.Id), 2),
                UserTotalOwing = Math.Round(validEvent.GetUserTotalOwing(user.Id), 2)
            });
        }



        [HttpPut("{eventId:long}")]
        public async Task<IActionResult> UpdateEvent(long eventId, [FromBody] UpdateEventDto dto)
        {
            // Return bad request if any of the required fields are left empty
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            GenericResponseDto? validationResponse = authService.ValidateJwtAndUserEventRelations(User, eventId, true, out User user, out Event validEvent);

            if (validationResponse != null)
                return Ok(validationResponse);

            try
            {
                Dictionary<long, User> userDict = userService.Users.ToDictionary(u => u.Id, u => u);
                bool success = await eventService.UpdateEvent(
                    validEvent,
                    dto.EventName,
                    dto.EventDate,
                    dto.Participants.Select(pId => userDict[pId]).ToList()
                );

                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = success,
                    ErrorMessage = success ? null : "Server Error: Failed to update event!"
                });
            }
            catch
            {
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "One or more participants are not valid users"
                });
            }
        }



        [HttpDelete("{eventId:long}")]
        public async Task<IActionResult> DeleteEvent(long eventId)
        {
            // Return bad request if any of the required fields are left empty
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            GenericResponseDto? validationResponse = authService.ValidateJwtAndUserEventRelations(User, eventId, true, out _, out Event validEvent);

            if (validationResponse != null)
                return Ok(validationResponse);

            bool success = await eventService.DeleteEvent(validEvent);

            return Ok(new GenericResponseDto()
            {
                ActionSuccess = success,
                ErrorMessage = success ? null : "Server Error: Failed to delete event"
            });
        }
    }
}
