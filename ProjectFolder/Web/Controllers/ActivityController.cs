//***********************************************************************************
//Program: ActivityController.cs
//Description: Handles activity endpoints
//Date: Oct 8, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.AspNetCore.Mvc;
using takethetab_server.Application;
using takethetab_server.Application.Dtos;
using takethetab_server.Application.Dtos.Activities.CreateActivity;
using takethetab_server.Application.Dtos.Activities.FetchActivity;
using takethetab_server.Application.Dtos.Activities.UpdateActivity;
using takethetab_server.Domain.Entities;
using takethetab_server.Web.Services;

namespace takethetab_server.Web.Controllers
{
    [ApiController]
    [Route("events/{eventId:long}/activities")]
    public class ActivityController(EventService eventService, UserService userService, AuthService authService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateActivity(long eventId, [FromBody] CreateActivityDto dto)
        {
            // Return bad request if any of the required fields are left empty
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            GenericResponseDto? validationResponse = authService.ValidateJwtAndUserEventRelations(User, eventId, false, out User user, out Event validEvent);

            if (validationResponse != null)
                return Ok(validationResponse);

            Dictionary<long, User> userDict = userService.Users.ToDictionary(u => u.Id, u => u);

            if (dto.GratuityAmount < 0)
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "Gratuity amount cannot be negative"
                });

            if (dto.Items.Count < 0)
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "Cannot create an event with no items"
                });

            foreach (CreateActivityItem item in dto.Items)
            {
                if (item.ItemName.Trim().Length == 0)
                    return Ok(new GenericResponseDto()
                    {
                        ActionSuccess = false,
                        ErrorMessage = "All items must have a name"
                    });

                if (item.ItemCost <= 0)
                    return Ok(new GenericResponseDto()
                    {
                        ActionSuccess = false,
                        ErrorMessage = "All items must have a valid cost"
                    });

                if (item.Payers.Count == 0)
                    return Ok(new GenericResponseDto()
                    {
                        ActionSuccess = false,
                        ErrorMessage = "All items must have a payer"
                    });
            }


            try
            {
                long? activityId = await eventService.CreateActivity(dto.ActivityName.Trim(), dto.IsGratuityTypePercent, dto.GratuityAmount, dto.IncludeTax, validEvent, user, dto.Items);

                if (activityId != null)
                    return Ok(new CreateActivityResponseDto()
                    {
                        ActionSuccess = true,
                        ErrorMessage = null,
                        ActivityId = activityId
                    });
            }
            catch
            {
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "One or more of the payers ids are invalid!"
                });
            }

            return Ok(new GenericResponseDto()
            {
                ActionSuccess = false,
                ErrorMessage = "Server Error: Failed to create activity!"
            });
        }



        [HttpDelete("{activityId:long}")]
        public async Task<IActionResult> DeleteActivity(long eventId, long activityId)
        {
            // Return bad request if any of the required fields are left empty
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            GenericResponseDto? validationResponse = authService.ValidateJwtAndUserEventRelations(User, eventId, false, out User user, out Event validEvent);

            if (validationResponse != null)
                return Ok(validationResponse);

            Activity? activity = validEvent.Activities.Find(a => a.Id == activityId);

            if (activity == null)
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "Activity with that id does not exist"
                });

            if (await eventService.DeleteActivity(validEvent, activity))
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = true,
                    ErrorMessage = null
                });

            return Ok(new GenericResponseDto()
            {
                ActionSuccess = false,
                ErrorMessage = "Server Error: Failed to delete activity"
            });
        }



        [HttpGet("{activityId:long}")]
        public IActionResult FetchActivity(long eventId, long activityId)
        {
            // Return bad request if any of the required fields are left empty
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            GenericResponseDto? validationResponse = authService.ValidateJwtAndUserEventRelations(User, eventId, false, out User user, out Event validEvent);

            if (validationResponse != null)
                return Ok(validationResponse);

            Activity? activity = eventService.FetchActivity(validEvent.Id, activityId);

            if (activity == null)
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "Activity does not exist!"
                });

            return Ok(new FetchActivityResponseDto()
            {
                ActionSuccess = true,
                ErrorMessage = null,
                CreatedActivity = activity.Payee.Id == user.Id,
                ActivityName = activity.Name,
                IsPayee = activity.Payee.Id == user.Id,
                Amount = activity.Payee.Id == user.Id ? activity.TotalAmountOwed(user.Id) : activity.TotalAmountOwing(user.Id),
                IsGratuityTypePercent = activity.IsGratuityTypePercent,
                GratuityAmount = activity.GratuityAmount,
                AddFivePercentTax = activity.AddFivePercentTax,
                ActivitySubtotal = activity.Items.Sum(i => i.Cost),
                Payers = activity.GetPayers().Select(p => new FetchActivityResponsePayer()
                {
                    PayerId = p.Payer.Id,
                    PayerEmail = p.Payer.Email,
                    PayerName = p.Payer.DisplayName,
                    AmountOwing = p.AmountOwing
                }).ToList(),
                Payee = new()
                {
                    PayeeName = activity.Payee.DisplayName,
                    PayerEmail = activity.Payee.Email,
                    PayerPhoneNumber = activity.Payee.PhoneNumber
                },
                Items = [.. activity.Items.Select(item => new FetchActivityResponseItem()
                    {
                        ItemId = item.Id,
                        ItemName = item.Name,
                        ItemCost = item.Cost,
                        IsSplitEvently = item.IsSplitTypeEvenly,
                        Payers = item.Payers.Select(p => new FetchActivityResponsePayer() {
                            PayerId = p.Payer.Id,
                            PayerEmail = p.Payer.Email,
                            PayerName = p.Payer.DisplayName,
                            AmountOwing = p.AmountOwing
                        }).ToList()
                    })]
            });
        }



        [HttpPut("{activityId:long}")]
        public async Task<IActionResult> UpdateActivity(long eventId, long activityId, [FromBody] UpdateActivityDto dto)
        {
            // Return bad request if any of the required fields are left empty
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            GenericResponseDto? validationResponse = authService.ValidateJwtAndUserEventRelations(User, eventId, false, out User user, out Event validEvent);

            if (validationResponse != null)
                return Ok(validationResponse);

            GenericResponseDto? activityValidationResponse = authService.ValidateActivityUserRelations(validEvent.Id, activityId, user.Id, true, out Activity validActivity);

            if (activityValidationResponse != null)
                return Ok(activityValidationResponse);

            bool success = await eventService.UpdateActivity(validActivity, dto.ActivityName.Trim(), dto.IsGratuityTypePercent, dto.GratuityAmount, dto.AddFivePercentTax, dto.Items);

            return Ok(new GenericResponseDto()
            {
                ActionSuccess = success,
                ErrorMessage = success ? null : "Server Error: Failed to update activity"
            });
        }
    }
}
