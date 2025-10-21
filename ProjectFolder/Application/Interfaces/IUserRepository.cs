//***********************************************************************************
//Program: IUserRepository.cs
//Description: Interface for the user repository
//Date: Sep 25, 2025
//Author: John Nasitem
//***********************************************************************************



using takethetab_server.Domain.Entities;

namespace takethetab_server.Application.Interfaces
{
    public interface IUserRepository
    {
        #region User
        /// <summary>
        /// Create a user
        /// </summary>
        /// <param name="displayName">Dispaly name of user</param>
        /// <param name="email">Email of user</param>
        /// <param name="hashedPassword">Hashed password of user</param>
        /// <param name="phoneNumber">Phone number of user</param>
        /// <returns><see cref="User"/> instance</returns>
        public Task<User> CreateUser(string displayName, string email, string hashedPassword, string? phoneNumber);



        /// <summary>
        /// Gets all users from the database
        /// </summary>
        /// <returns>List of users from the database</returns>
        public Task<List<User>> GetUsers();



        /// <summary>
        /// Update a user 
        /// </summary>
        /// <param name="updatedUser">User with updated values</param>
        public Task UpdateUser(User updatedUser);



        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="userId">Id of user to delete</param>
        public Task DeleteUser(long userId);
        #endregion



        #region Friend
        /// <summary>
        /// Add a friend to the user
        /// </summary>
        /// <param name="userId">Id of user</param>
        /// <param name="friendIdToAdd">Id of user to add as a friend</param>
        public Task AddFriend(long userId, long friendIdToAdd);



        /// <summary>
        /// Add a friend from the user
        /// </summary>
        /// <param name="userId">Id of user</param>
        /// <param name="friendIdToAdd">Id of user to add as a friend</param>
        public Task RemoveFriend(long userId, long friendIdToRemove);



        /// <summary>
        /// Add friend request
        /// </summary>
        /// <param name="userIdSending">Id of user sending the friend request</param>
        /// <param name="userIdRecieving">Id of user receiving the friend request</param>
        /// <returns></returns>
        public Task AddFriendRequest(long userIdSending, long userIdRecieving);



        /// <summary>
        /// Decline a friend request
        /// </summary>
        /// <param name="userId">Id of user</param>
        /// <param name="otherUserId">Id of user to decline a friend request from</param>
        public Task DeclineFriendRequest(long userId, long otherUserId);
        #endregion
    }
}
