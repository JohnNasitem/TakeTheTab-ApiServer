//***********************************************************************************
//Program: RefreshTokenRepository.cs
//Description: Repository for refresh tokens
//Date: Sep 28, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.Options;
using System.Data.SQLite;
using takethetab_server.Application;
using takethetab_server.Application.Interfaces;
using takethetab_server.Domain.Entities;
using takethetab_server.Infrastructure.Database;
using static takethetab_server.Infrastructure.Database.DatabaseInitializer;

namespace takethetab_server.Infrastructure.Repositories
{
    public class RefreshTokenRepository(DatabaseInitializer dbInit, UserService userSerivce, IOptions<TokenConfig> config) : IRefreshTokenRepository
    {
        // Connection to database
        private readonly SQLiteConnection _sqlConnection = dbInit.SqlConnection;



        /// <summary>
        /// Delete all expired refresh tokens
        /// </summary>
        public async Task PruneExpiredTokens()
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.RefreshTokens)} WHERE ExpiryDate <= @dateTimeNow";
            cmd.Parameters.AddWithValue("@dateTimeNow", DateTime.UtcNow);
            await cmd.ExecuteNonQueryAsync();
        }



        /// <summary>
        /// Get refresh token from the database
        /// </summary>
        /// <param name="refreshToken">Refresh token to get</param>
        /// <returns><see cref="RefreshToken"/> instance if its valid, otherwise null</returns>
        public async Task<RefreshToken?> GetRefreshToken(string refreshToken)
        {
            RefreshToken? refreshTokenInstance = null;

            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"SELECT * FROM {Enum.GetName(DatabaseTables.RefreshTokens)} WHERE Token = @refreshToken";
            cmd.Parameters.AddWithValue("@refreshToken", refreshToken);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                refreshTokenInstance = new RefreshToken(
                    token: reader["Token"].ToString()!,
                    // User should always exist because all refresh tokens are deleted along side the user
                    user: userSerivce.Users.Find(u => u.Id == long.Parse(reader["UserId"].ToString()!))!,
                    browserId: reader["BrowserId"].ToString()!,
                    creationDate: DateTime.Parse(reader["CreationDate"].ToString()!),
                    expiryDate: DateTime.Parse(reader["ExpiryDate"].ToString()!)
                );
            }

            return refreshTokenInstance;
        }



        /// <summary>
        /// Add refresh token to the database<br/>
        /// If an entry with the same user and browser id exist then it will be deleted
        /// </summary>
        /// <param name="token">Refresh token</param>
        /// <param name="userId">User connected to the token</param>
        /// <param name="browserId">Browser id of token</param>
        /// <returns><see cref="RefreshToken"/> instance</returns>
        public async Task CreateRefreshToken(string token, long userId, string browserId)
        {
            using SQLiteTransaction transaciton = _sqlConnection.BeginTransaction();
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.Transaction = transaciton;

            try
            {
                // Delete any exist tokens that have the same token and browser id
                cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.RefreshTokens)} WHERE UserId = @userId AND BrowserId = @browserId";
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@browserId", browserId);
                await cmd.ExecuteNonQueryAsync();

                // Add token to the database
                cmd.CommandText = $@"INSERT INTO {Enum.GetName(DatabaseTables.RefreshTokens)} (Token, UserId, BrowserId, CreationDate, ExpiryDate) VALUES (@token, @userId, @browserId, @creationDate, @expiryDate)";
                cmd.Parameters.AddWithValue("@token", token);
                cmd.Parameters.AddWithValue("@creationDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@expiryDate", DateTime.Now.AddDays(config.Value.RefreshTokenLifeSpanDays));
                await cmd.ExecuteNonQueryAsync();

                transaciton.Commit();
            }
            catch
            {
                transaciton.Rollback();
                throw;
            }
        }
    }
}
