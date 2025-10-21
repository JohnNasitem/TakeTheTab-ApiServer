//***********************************************************************************
//Program: AuthService.cs
//Description: Handles verifying request sender identity
//Date: Oct 8, 2025
//Author: John Nasitem
//***********************************************************************************



using System.Security.Claims;
using takethetab_server.Application;
using takethetab_server.Application.Dtos;
using takethetab_server.Domain.Entities;

namespace takethetab_server.Web.Services
{
    public class AuthService(UserService userService, EventService eventService)
    {
        /// <summary>
        /// Extract user from the ControllerBase user
        /// </summary>
        /// <param name="controllerUser">ControllerBase user to extract from</param>
        /// <param name="user">User extracted</param>
        /// <returns>Null if user was successfully extracted, otherwise a response dto to return</returns>
        public GenericResponseDto? ValidateJwtTokenUser(ClaimsPrincipal controllerUser, out User user)
        {
            user = null!;

            try
            {
                user = userService.Users.Find(u => u.Id == long.Parse(controllerUser.FindFirstValue(ClaimTypes.NameIdentifier)!))!;
            }
            catch
            {
                return new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "Server Error: Jwt token is invalid"
                };
            }

            if (user == null)
                return new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "Your user id isn't valid!"
                };

            return null;
        }



        /// <summary>
        /// Validate an event if a user is related to it
        /// </summary>
        /// <param name="eventId">Id of event to validated</param>
        /// <param name="userId">Id of user to check</param>
        /// <param name="validEvent">Event extracted</param>
        /// <returns>Null if event was successfully extracted, otherwise a response dto to return</returns>
        public GenericResponseDto? ValidateUserEventRelations(long eventId, long userId, bool doesUserNeedToBeCreator, out Event validEvent)
        {
            validEvent = eventService.FetchEvent(eventId)!;

            if (validEvent == null)
                return new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "Event with that id does not exist"
                };

            // Check if requesting user is a participant
            if (validEvent.Creator.Id != userId && !validEvent.Participants.Any(u => u.Id == userId))
            {
                validEvent = null!;
                return new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "You have no access to that event!"
                };
            }

            // Check if users needs to be the creator of the event
            if (doesUserNeedToBeCreator && validEvent.Creator.Id != userId)
            {
                validEvent = null!;
                return new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "You do not have permission to modify the event!"
                };
            }

            return null;
        }



        /// <summary>
        /// Extract user from the ControllerBase user and validates an event if a user is related to it
        /// </summary>
        /// <param name="controllerUser">ControllerBase user to extract from</param>
        /// <param name="eventId">Id of event to validated</param>
        /// <param name="doesUserNeedToBeCreator">Does the user need to be the creator of the event/param>
        /// <param name="validUser">User extracted</param>
        /// <param name="validEvent">Event extracted</param>
        /// <returns>Null if user and event was successfully extracted, otherwise a response dto to return</returns>
        public GenericResponseDto? ValidateJwtAndUserEventRelations(ClaimsPrincipal controllerUser, long eventId, bool doesUserNeedToBeCreator, out User validUser, out Event validEvent)
        {
            validEvent = null!;

            GenericResponseDto? userValidationResponse = ValidateJwtTokenUser(controllerUser, out validUser);

            if (userValidationResponse != null)
                return userValidationResponse;

            GenericResponseDto? eventValidationResponse = ValidateUserEventRelations(eventId, validUser.Id, doesUserNeedToBeCreator, out validEvent);

            if (eventValidationResponse != null)
                return eventValidationResponse;

            return null;
        }



        /// <summary>
        /// Validate an activity
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="activityId"></param>
        /// <param name="doesUserNeedToBeCreator"></param>
        /// <param name="activity"></param>
        /// <returns>Null if activity was successfully extracted, otherwise a response dto to return</returns>
        public GenericResponseDto? ValidateActivityUserRelations(long eventId, long activityId, long userId, bool doesUserNeedToBeCreator, out Activity activity)
        {
            activity = eventService.FetchActivity(eventId, activityId)!;

            if (activity == null)
                return new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "Activity does not exist!"
                };

            if (doesUserNeedToBeCreator && activity.Payee.Id != userId)
            {
                activity = null!;
                return new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "You do not have the permission to edit this activity"
                };
            }

            return null;
        }
    }
}
