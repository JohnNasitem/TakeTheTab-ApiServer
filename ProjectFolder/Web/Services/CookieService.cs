//***********************************************************************************
//Program: CookieService.cs
//Description: Handles http only cookies
//Date: Oct 4, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.Options;
using takethetab_server.Application.Interfaces;
using takethetab_server.Domain.Entities;

namespace takethetab_server.Web.Services
{
    public class CookieService(ITokenService tokenService, IOptions<TokenConfig> config)
    {
        /// <summary>
        /// Refresh http only cookies using the existing refresh token
        /// </summary>
        /// <param name="response">Response to append cookies to</param>
        /// <param name="response">Http response</param>
        /// <param name="refreshToken">Exisiting refresh token</param>
        /// <returns>Access token expiration time if refresh succeeded, otherwise null</returns>
        public async Task<bool> RefreshHttpOnlyCookies(HttpResponse response, string refreshToken)
        {
            RefreshToken? token = await tokenService.ValidateRefreshToken(refreshToken);

            if (token == null)
                return false;

            await AddHttpOnlyCookies(response, token.User.Id, token.BrowserId);
            return true;
        }



        /// <summary>
        /// Add the refresh and access tokens as 
        /// </summary>
        /// <param name="response">Response to append cookies to</param>
        /// <param name="userId">ID of user</param>
        /// <param name="browserId">Id of browser</param>
        public async Task AddHttpOnlyCookies(HttpResponse response, long userId, string browserId)
        {
            await AppendCookies(response, userId, browserId);
        }



        /// <summary>
        /// Clears the token cookies from the http response
        /// </summary>
        /// <param name="response">Response to clear</param>
        public async Task ClearHttpOnlyCookies(HttpResponse response)
        {
            await AppendCookies(response, null, null);
        }



        /// <summary>
        /// Append cookies to the given http response
        /// </summary>
        /// <param name="response">Response to append cookies to</param>
        /// <param name="userId">ID of user</param>
        /// <param name="browserId">Id of browser</param>
        private async Task AppendCookies(HttpResponse response, long? userId, string? browserId)
        {
            bool shouldClearCookie = userId == null;
            string refreshToken = shouldClearCookie ? "" : await tokenService.GenerateRefreshToken((long)userId!, browserId!);
            string accessToken = shouldClearCookie ? "" : tokenService.GenerateAccessToken((long)userId!);

            response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,                                                            // JS cannot read it
                Secure = true,                                                              // only sent over HTTPS
                SameSite = SameSiteMode.Lax,                                             // prevents CSRF in most cases
                Domain = ".takethetab.com",
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(shouldClearCookie ? -1 : config.Value.RefreshTokenLifeSpanDays)
            });

            response.Cookies.Append("accessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,                                                            // JS cannot read it
                Secure = true,                                                              // only sent over HTTPS
                SameSite = SameSiteMode.Lax,                                             // prevents CSRF in most cases
                Domain = ".takethetab.com",
                Path = "/",
                Expires = DateTime.UtcNow.AddHours(shouldClearCookie ? -1 : config.Value.AccessTokenLifeSpanHours)
            });
        }
    }
}
