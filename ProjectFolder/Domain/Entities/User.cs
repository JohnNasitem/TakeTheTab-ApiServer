//***********************************************************************************
//Program: User.cs
//Description: User model
//Date: Sep 25, 2025
//Author: John Nasitem
//***********************************************************************************



namespace takethetab_server.Domain.Entities
{
    public class User(long id, string displayName, string email, string hashedPassword, string? phoneNumber, List<User> friends, List<User> incomingFriendRequests, List<User> outgoingFriendRequests)
    {
        /// <summary>
        /// User's id
        /// </summary>
        public long Id { get; set; } = id;



        /// <summary>
        /// Name displayed to user and friends
        /// </summary>
        public string DisplayName { get; set; } = displayName;



        /// <summary>
        /// User's email
        /// </summary>
        public string Email { get; set; } = email;



        /// <summary>
        /// User's hashed password
        /// </summary>
        public string HashedPassword { get; set; } = hashedPassword;



        /// <summary>
        /// User's phone number
        /// </summary>
        public string? PhoneNumber { get; set; } = phoneNumber;



        /// <summary>
        /// User's friends
        /// </summary>
        public List<User> Friends { get; set; } = friends;



        /// <summary>
        /// Users who send this user a friend request
        /// </summary>
        public List<User> IncomingFriendRequests { get; set; } = incomingFriendRequests;



        /// <summary>
        /// Users who this user sent a friend request to
        /// </summary>
        public List<User> OutgoingFriendRequests { get; set; } = outgoingFriendRequests;
    }
}
