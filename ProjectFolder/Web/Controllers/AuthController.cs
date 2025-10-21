//***********************************************************************************
//Program: AuthController.cs
//Description: Handles authentication and authorization endpoints
//Date: Oct 8, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using takethetab_server.Application;
using takethetab_server.Application.Dtos;
using takethetab_server.Application.Dtos.Auth.LoginUser;
using takethetab_server.Application.Dtos.Auth.RegisterUser;
using takethetab_server.Application.Interfaces;
using takethetab_server.Domain.Entities;
using takethetab_server.Web.Services;

namespace takethetab_server.Web.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController(UserService userService, ITokenService tokenSerivce, CookieService cookieService) : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshTokens()
        {
            // Check for a refresh token
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
                return Unauthorized();

            // Dont refresh tokens if access token is still valid
            if (Request.Cookies.TryGetValue("accessToken", out var accessToken))
            {
                DateTime? expiration = tokenSerivce.GetAccessTokenExpiry(accessToken);

                if (expiration != null && expiration >= DateTime.Now)
                    return Ok(new GenericResponseDto()
                    {
                        ActionSuccess = true,
                        ErrorMessage = null
                    });
            }

            RefreshToken? token = await tokenSerivce.ValidateRefreshToken(refreshToken);
            bool isTokenValid = token != null;

            if (isTokenValid)
                await cookieService.RefreshHttpOnlyCookies(Response, token!.Token);

            return Ok(new GenericResponseDto()
            {
                ActionSuccess = isTokenValid,
                ErrorMessage = isTokenValid ? null : "Refresh token is invalid!"
            });
        }



        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUser dto)
        {
            // Return bad request if any of the required fields are left empty
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool userCreated = await userService.CreateUser(dto.DisplayName, dto.Email, dto.Password, dto.PhoneNumber);

            return Ok(new GenericResponseDto()
            {
                ActionSuccess = userCreated,
                ErrorMessage = userCreated ? null : "Email is already in use!"
            });
        }



        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginUserDto dto)
        {
            // Return bad request if any of the required fields are left empty
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            long? userId = userService.VerifyUserLoginCredentials(dto.Email, dto.Password);
            bool isLogInSuccessful = userId != null;

            if (isLogInSuccessful)
                await cookieService.AddHttpOnlyCookies(Response, (long)userId!, dto.BrowserId);

            return Ok(new GenericResponseDto()
            {
                ActionSuccess = isLogInSuccessful,
                ErrorMessage = isLogInSuccessful ? null : "Log in credentials are invalid!"
            });
        }



        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutUser()
        {
            await cookieService.ClearHttpOnlyCookies(Response);
            return Ok(new GenericResponseDto()
            {
                ActionSuccess = true,
                ErrorMessage = null
            });
        }
    }
}
