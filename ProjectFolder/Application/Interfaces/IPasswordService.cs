//***********************************************************************************
//Program: IPasswordService.cs
//Description: Interface for password methods
//Date: Sep 28, 2025
//Author: John Nasitem
//***********************************************************************************



namespace takethetab_server.Application.Interfaces
{
    public interface IPasswordService
    {
        /// <summary>
        /// Hash a password
        /// </summary>
        /// <param name="password">Password to hash</param>
        /// <returns>Hashed password</returns>
        public string HashPassword(string password);




        /// <summary>
        /// Check if the password matches the hash
        /// </summary>
        /// <param name="password">Password to check</param>
        /// <param name="hash">Hash to compare to</param>
        /// <returns>True if password matches, otherwise false</returns>
        public bool VerifyPassword(string password, string hash);
    }
}
