//***********************************************************************************
//Program: PasswordService.cs
//Description: Handles password hashing and verification
//Date: Sep 28, 2025
//Author: John Nasitem
//***********************************************************************************



using takethetab_server.Application.Interfaces;

namespace takethetab_server.Infrastructure.Password
{
    public class PasswordService : IPasswordService
    {
        /// <summary>
        /// Hash a password
        /// </summary>
        /// <param name="password">Password to hash</param>
        /// <returns>Hashed password</returns>
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }



        /// <summary>
        /// Check if the password matches the hash
        /// </summary>
        /// <param name="password">Password to check</param>
        /// <param name="hash">Hash to compare to</param>
        /// <returns>True if password matches, otherwise false</returns>
        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
