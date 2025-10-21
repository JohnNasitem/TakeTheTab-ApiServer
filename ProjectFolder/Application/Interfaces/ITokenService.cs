//***********************************************************************************
//Program: ITokenService.cs
//Description: Interface for token generation methods
//Date: Sep 28, 2025
//Author: John Nasitem
//***********************************************************************************



using takethetab_server.Domain.Entities;

namespace takethetab_server.Application.Interfaces
{
    public interface ITokenService
    {
        /// <summary>
        /// Generate an access token
        /// </summary>
        /// <param name="userId">Id of user the token is for</param>
        /// <returns>Access Token</returns>
        public string GenerateAccessToken(long userId);



        /// <summary>
        /// Generate a refresh token and add it to the database<br/>
        /// Deletes any existing refresh token if it has the same user and browser id
        /// </summary>
        /// <param name="userId">Id of user the token is for</param>
        /// <param name="browserId">Id of browser the token is for</param>
        /// <returns>Refresh Token</returns>
        public Task<string> GenerateRefreshToken(long userId, string browserId);



        /// <summary>
        /// Validates the specified refresh token
        /// </summary>
        /// <param name="refreshTokenString">Refresh token to validate</param>
        /// <returns><see cref="RefreshToken"/> instance if the token is valid, otherwise null</returns>
        public Task<RefreshToken?> ValidateRefreshToken(string refreshTokenString);



        /// <summary>
        /// Get expiration of access token
        /// </summary>
        /// <param name="token">JWT access token</param>
        /// <returns>Expiration of token if it is valid, otherwise false</returns>
        public DateTime? GetAccessTokenExpiry(string token);
    }
}
