//***********************************************************************************
//Program: UserController.cs
//Description: Handles account endpoints
//Date: Sep 25, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using takethetab_server.Application;
using takethetab_server.Application.Dtos;
using takethetab_server.Application.Dtos.Users.EditUser;
using takethetab_server.Application.Dtos.Users.FetchUser;
using takethetab_server.Domain.Entities;
using static takethetab_server.Application.UserService;

namespace takethetab_server.Web.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController(UserService userService, EventService eventService) : ControllerBase
    {
        [HttpGet]
        public IActionResult FetchUser()
        {
            User? user = null;
            string? errorMessage = null;
            List<Event>? events = null;
            bool success = false;

            try
            {
                user = userService.Users.Find(u => u.Id == long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!))!;
                events = eventService.GetUserEvents(user.Id);
                success = true;
            }
            catch
            {
                errorMessage = "Problem happened with fetching user";
            }

            return Ok(new FetchUserResponseDto()
            {
                ActionSuccess = success,
                ErrorMessage = errorMessage,
                UserId = success ? user!.Id : null,
                UserDisplayName = success ? user!.DisplayName : null,
                UserEmail = success ? user!.Email : null,
                Events = events?.OrderBy(e => e.Date).ToDictionary(e => e.Id, e =>
                    new FetchUserResponseEvent()
                    {
                        EventName = e.Name,
                        UserTotalOwed = Math.Round(e.GetUserTotalOwed(user!.Id), 2),
                        UserTotalOwing = Math.Round(e.GetUserTotalOwing(user!.Id), 2)
                    }),
                UserTotalOwed = success ? Math.Round(events!.Sum(e => e.GetUserTotalOwed(user!.Id)), 2) : null,
                UserTotalOwing = success ? Math.Round(events!.Sum(e => e.GetUserTotalOwing(user!.Id)), 2) : null
            });
        }



        [HttpPut]
        public async Task<IActionResult> EditUserProfile([FromBody] EditUserProfileDto dto)
        {
            // Return bad request if any of the required fields are left empty
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool success = false;
            string? errorMessage = null;

            try
            {
                success = await userService.EditUserProfile(long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!), dto.DisplayName, dto.Email);
            }
            catch
            {
                errorMessage = "Problem happened with editting the user";
            }

            return Ok(new GenericResponseDto()
            {
                ActionSuccess = success,
                ErrorMessage = errorMessage
            });
        }



        [HttpPut("password")]
        public async Task<IActionResult> EditUserPassword([FromBody] EditUserPasswordDto dto)
        {
            // Return bad request if any of the required fields are left empty
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            EditUserPasswordResponse response;
            string? errorMessage = null;

            response = await userService.EditUserPassword(long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!), dto.OldPassword, dto.NewPassword);

            switch (response)
            {
                case EditUserPasswordResponse.Success:
                    errorMessage = null;
                    break;
                case EditUserPasswordResponse.OldPasswordIncorrect:
                    errorMessage = "Old password is incorrect!";
                    break;
                case EditUserPasswordResponse.UserNotFound:
                case EditUserPasswordResponse.ServerError:
                    errorMessage = "Something went wrong with the server";
                    break;
            }

            return Ok(new GenericResponseDto()
            {
                ActionSuccess = response == UserService.EditUserPasswordResponse.Success,
                ErrorMessage = errorMessage
            });
        }
    }
}
