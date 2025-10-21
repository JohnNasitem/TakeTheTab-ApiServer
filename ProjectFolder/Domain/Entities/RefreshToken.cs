//***********************************************************************************
//Program: RefreshToken.cs
//Description: Refresh Token model
//Date: Sep 28, 2025
//Author: John Nasitem
//***********************************************************************************



namespace takethetab_server.Domain.Entities
{
    public class RefreshToken(string token, User user, string browserId, DateTime creationDate, DateTime expiryDate)
    {
        /// <summary>
        /// Refresh token
        /// </summary>
        public string Token { get; set; } = token;



        /// <summary>
        /// User associated with the token
        /// </summary>
        public User User { get; set; } = user;



        /// <summary>
        /// Unique id for the browser/device combo used
        /// </summary>
        public string BrowserId { get; set; } = browserId;



        /// <summary>
        /// Date when the token was created
        /// </summary>
        public DateTime CreationDate { get; set; } = creationDate;



        /// <summary>
        /// Date when the token expires
        /// </summary>
        public DateTime ExpiryDate { get; set; } = expiryDate;
    }
}
