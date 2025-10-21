//***********************************************************************************
//Program: FriendsController.cs
//Description: Handles user friends endpoints
//Date: Oct 8, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using takethetab_server.Application;
using takethetab_server.Application.Dtos;
using takethetab_server.Application.Dtos.Friends.RepondToRequest;
using takethetab_server.Application.Dtos.Friends.SendRequest;
using takethetab_server.Application.Dtos.Users.FetchUser;
using takethetab_server.Domain.Entities;
using static takethetab_server.Application.UserService;

namespace takethetab_server.Web.Controllers
{
    [ApiController]
    [Route("friends")]
    public class FriendsController(UserService userService) : ControllerBase
    {
        [HttpGet]
        public IActionResult FetchUserFriends()
        {
            User? user = null;
            string? errorMessage = null;

            try
            {
                user = userService.Users.Find(u => u.Id == long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!))!;
            }
            catch
            {
                errorMessage = "Problem happened with fetching friends";
            }

            return Ok(new FetchUserFriendsResponseDto()
            {
                ActionSuccess = user != null,
                ErrorMessage = errorMessage,
                Friends = user?.Friends?.ToDictionary(f => f.Id, f => new string[] { f.DisplayName, f.Email }),
                IncommingFriendRequests = user?.IncomingFriendRequests?.ToDictionary(f => f.Id, f => new string[] { f.DisplayName, f.Email })
            });
        }



        [HttpPost]
        public async Task<IActionResult> SendFriendRequest([FromBody] SendFriendRequestDto dto)
        {
            // Return bad request if any of the required fields are left empty
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            User user = null!;
            User otherUser = null!;
            bool gotUsersSuccessfully = true;

            try
            {
                user = userService.Users.Find(u => u.Id == long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!))!;
                otherUser = userService.Users.Find(u => u.Email.Equals(dto.OtherUserEmail, StringComparison.CurrentCultureIgnoreCase))!;
            }
            catch
            {
                gotUsersSuccessfully = false;
            }

            if (!gotUsersSuccessfully)
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "Server Error: Failed to get users!"
                });

            if (otherUser == null)
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "An account with that email does not exist!"
                });

            if (user.Email.Equals(otherUser.Email, StringComparison.CurrentCultureIgnoreCase))
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "Cannot send a friend request to yourself!"
                });

            if (user.Friends.Any(f => f.Id == otherUser.Id))
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "This person is already your friend!"
                });

            if (user.OutgoingFriendRequests.Any(f => f.Id == otherUser.Id))
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "Already sent a friend request to this person"
                });

            // SendFriendRequest will also accept friend requests if the otherUser sent a friend request already
            SendFriendRequestReponse response = await userService.SendFriendRequest(user, otherUser);

            return response switch
            {
                SendFriendRequestReponse.SentRequest => Ok(new GenericResponseDto()
                {
                    ActionSuccess = true,
                    ErrorMessage = "Sent Friend Request"
                }),
                SendFriendRequestReponse.AccpetedFriendRequest => Ok(new GenericResponseDto()
                {
                    ActionSuccess = true,
                    ErrorMessage = "Accepted Friend Request"
                }),
                _ => Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "Server Error: Failed to send friend request!"
                }),
            };
        }



        [HttpPut]
        public async Task<IActionResult> RepondToFriendRequest([FromBody] RespondToFriendRequestDto dto)
        {
            // Return bad request if any of the required fields are left empty
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            User user = null!;
            User otherUser = null!;
            bool gotUsersSuccessfully = true;

            try
            {
                user = userService.Users.Find(u => u.Id == long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!))!;
                otherUser = userService.Users.Find(u => u.Id == dto.OtherUserId)!;
            }
            catch
            {
                gotUsersSuccessfully = false;
            }

            if (!gotUsersSuccessfully)
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "Server Error: Failed to get users"
                });

            if (otherUser == null)
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "An account with that email does not exist!"
                });

            if (user.Email.Equals(otherUser.Email, StringComparison.CurrentCultureIgnoreCase))
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "Cannot respond to a friend request from yourself"
                });

            if (user.Friends.Any(f => f.Id == otherUser.Id))
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "This person is already your friend!"
                });

            if (!otherUser.OutgoingFriendRequests.Any(f => f.Id == user.Id))
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "This user did not send you a friend request"
                });

            bool success = await userService.RespondToFriendRequest(otherUser, user, dto.AcceptedRequest);

            return Ok(new GenericResponseDto()
            {
                ActionSuccess = success,
                ErrorMessage = success ? null : "Failed to respond to friend request!"
            });
        }



        [HttpDelete("{friendId:long}")]
        public async Task<IActionResult> RemoveFriend(long friendId)
        {
            // Return bad request if any of the required fields are left empty
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            User user = null!;
            User otherUser = null!;
            bool gotUsersSuccessfully = true;

            try
            {
                user = userService.Users.Find(u => u.Id == long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!))!;
                otherUser = userService.Users.Find(u => u.Id == friendId)!;
            }
            catch
            {
                gotUsersSuccessfully = false;
            }

            if (!gotUsersSuccessfully)
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "Server Error: Failed to get users"
                });

            if (otherUser == null)
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "An account with that email does not exist!"
                });

            if (user.Email.Equals(otherUser.Email, StringComparison.CurrentCultureIgnoreCase))
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "Cannot respond remove yourself as a friend"
                });

            if (!user.Friends.Any(f => f.Id == otherUser.Id))
                return Ok(new GenericResponseDto()
                {
                    ActionSuccess = false,
                    ErrorMessage = "This person is not currently your friend!"
                });

            bool success = await userService.RemoveFriend(user, otherUser);

            return Ok(new GenericResponseDto()
            {
                ActionSuccess = success,
                ErrorMessage = success ? null : "Failed to remove friend!"
            });
        }
    }
}
