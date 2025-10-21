//***********************************************************************************
//Program: IRefreshTokenRepository.cs
//Description: Interface for the token repository
//Date: Sep 28, 2025
//Author: John Nasitem
//***********************************************************************************



using takethetab_server.Domain.Entities;

namespace takethetab_server.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        /// <summary>
        /// Delete all expired refresh tokens
        /// </summary>
        public Task PruneExpiredTokens();



        /// <summary>
        /// Get refresh token from the database
        /// </summary>
        /// <param name="refreshToken">Refresh token to get</param>
        /// <returns><see cref="RefreshToken"/> instance if its valid, otherwise null</returns>
        public Task<RefreshToken?> GetRefreshToken(string refreshToken);



        /// <summary>
        /// Add refresh token to the database<br/>
        /// If an entry with the same user and browser id exist then it will be deleted
        /// </summary>
        /// <param name="token">Refresh token</param>
        /// <param name="userId">Id of user connected to the token</param>
        /// <param name="browserId">Browser id of token</param>
        public Task CreateRefreshToken(string token, long userId, string browserId);
    }
}
