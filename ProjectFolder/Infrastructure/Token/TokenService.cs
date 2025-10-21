//***********************************************************************************
//Program: ITokenService.cs
//Description: Handles token generation and verification
//Date: Sep 28, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using takethetab_server.Application.Interfaces;
using takethetab_server.Domain.Entities;

namespace takethetab_server.Infrastructure.Token
{
    public class TokenService(IRefreshTokenRepository refreshTokenRepository, IOptions<TokenConfig> config) : ITokenService
    {
        private readonly TokenConfig _config = config.Value;



        /// <summary>
        /// Generate an access token
        /// </summary>
        /// <param name="userId">Id of user the token is for</param>
        /// <returns>Access Token</returns>
        public string GenerateAccessToken(long userId)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable(_config.JwtEnvironmentVariableName)!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config.JwtIssuer,
                audience: _config.JwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_config.AccessTokenLifeSpanHours),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        /// <summary>
        /// Generate a refresh token and add it to the database<br/>
        /// Deletes any existing refresh token if it has the same user and browser id
        /// </summary>
        /// <param name="userId">Id of user the token is for</param>
        /// <param name="browserId">Id of browser the token is for</param>
        /// <returns>Refresh Token</returns>
        public async Task<string> GenerateRefreshToken(long userId, string browserId)
        {
            // Generate refresh token
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            string refreshToken = Convert.ToBase64String(randomBytes);

            // Add to the database
            await refreshTokenRepository.CreateRefreshToken(refreshToken, userId, browserId);

            return refreshToken;
        }



        /// <summary>
        /// Validates the specified refresh token
        /// </summary>
        /// <param name="refreshTokenString">Refresh token to validate</param>
        /// <returns><see cref="RefreshToken"/> instance if the token is valid, otherwise null</returns>
        public async Task<RefreshToken?> ValidateRefreshToken(string refreshTokenString)
        {
            RefreshToken? refreshToken = await refreshTokenRepository.GetRefreshToken(refreshTokenString);

            if (refreshToken == null)
                return null;

            // Remove refresh token if its expired
            if (DateTime.UtcNow < refreshToken.ExpiryDate)
            {
                _ = refreshTokenRepository.PruneExpiredTokens();
                return null;
            }

            return refreshToken;
        }



        /// <summary>
        /// Get expiration of access token
        /// </summary>
        /// <param name="token">JWT access token</param>
        /// <returns>Expiration of token if it is valid, otherwise false</returns>
        public DateTime? GetAccessTokenExpiry(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            // Extract expiration claim
            JwtSecurityToken jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            long? expClaim = jwtToken.Payload.Expiration;

            if (expClaim == null)
                return null;

            // Convert seconds to DateTime
            return DateTimeOffset.FromUnixTimeSeconds((long)expClaim).UtcDateTime;
        }
    }
}
