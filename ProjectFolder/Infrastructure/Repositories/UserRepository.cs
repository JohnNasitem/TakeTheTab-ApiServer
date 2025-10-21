//***********************************************************************************
//Program: EventRepository.cs
//Description: Repository for events and activities
//Date: Sep 25, 2025
//Author: John Nasitem
//***********************************************************************************



using System.Data.SQLite;
using takethetab_server.Application.Interfaces;
using takethetab_server.Domain.Entities;
using takethetab_server.Infrastructure.Database;
using static takethetab_server.Infrastructure.Database.DatabaseInitializer;



namespace takethetab_server.Infrastructure.Repositories
{
    public class UserRepository(DatabaseInitializer dbInit) : IUserRepository
    {
        // Connection to database
        private readonly SQLiteConnection _sqlConnection = dbInit.SqlConnection;



        #region User
        /// <summary>
        /// Create a user
        /// </summary>
        /// <param name="displayName">Dispaly name of user</param>
        /// <param name="email">Email of user</param>
        /// <param name="hashedPassword">Hashed password of user</param>
        /// <param name="phoneNumber">Phone number of user</param>
        /// <returns><see cref="User"/> instance</returns>
        public async Task<User> CreateUser(string displayName, string email, string hashedPassword, string? phoneNumber)
        {
            // Add user to the database
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"INSERT INTO {Enum.GetName(DatabaseTables.Users)} (DisplayName, Email, HashedPassword, PhoneNumber) VALUES (@displayName, @email, @hashedPassword, @phoneNumber)";
            cmd.Parameters.AddWithValue("@displayName", displayName);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@hashedPassword", hashedPassword);
            cmd.Parameters.AddWithValue("@phoneNumber", phoneNumber ?? (object)DBNull.Value);
            await cmd.ExecuteNonQueryAsync();

            // Get user's id
            cmd.CommandText = "SELECT last_insert_rowid();";
            long userid = Convert.ToInt64(await cmd.ExecuteScalarAsync());

            return new User(userid, displayName, email, hashedPassword, phoneNumber, [], [], []);
        }



        /// <summary>
        /// Gets all users from the database
        /// </summary>
        /// <returns>List of users from the database</returns>
        public async Task<List<User>> GetUsers()
        {
            List<User> users = [];

            using (SQLiteCommand cmd = _sqlConnection.CreateCommand())
            {
                // Get users
                cmd.CommandText = $@"SELECT * FROM {Enum.GetName(DatabaseTables.Users)}";
                using var usersReader = await cmd.ExecuteReaderAsync();
                while (await usersReader.ReadAsync())
                {
                    users.Add(new User(
                        id: long.Parse(usersReader["UserId"].ToString()!),
                        displayName: usersReader["DisplayName"].ToString()!,
                        email: usersReader["Email"].ToString()!,
                        hashedPassword: usersReader["HashedPassword"].ToString()!,
                        phoneNumber: usersReader["PhoneNumber"].ToString()!,
                        [],
                        [],
                        []
                    ));
                }

                await usersReader.CloseAsync();

                // Populate Friends property
                cmd.CommandText = $@"SELECT * FROM {Enum.GetName(DatabaseTables.UserConnections)}";
                using var friendsReader = await cmd.ExecuteReaderAsync();
                while (await friendsReader.ReadAsync())
                {
                    long userId = long.Parse(friendsReader["UserId"].ToString()!);
                    long otherUserId = long.Parse(friendsReader["OtherUserId"].ToString()!);
                    UserConnectionTypes connectionType = Enum.Parse<UserConnectionTypes>(friendsReader["ConnectionType"].ToString()!);

                    User user = null!;
                    User otherUser = null!;

                    foreach (User u in users)
                    {
                        if (u.Id == userId)
                            user = u;
                        else if (u.Id == otherUserId)
                            otherUser = u;
                    }

                    switch (connectionType)
                    {
                        case UserConnectionTypes.Friends:
                            user.Friends.Add(otherUser);
                            otherUser.Friends.Add(user);
                            break;
                        case UserConnectionTypes.SentFriendRequest:
                            user.OutgoingFriendRequests.Add(otherUser);
                            otherUser.IncomingFriendRequests.Add(user);
                            break;
                    }
                }
            }

            return users;
        }



        /// <summary>
        /// Update a user 
        /// </summary>
        /// <param name="updatedUser">User with updated values</param>
        public async Task UpdateUser(User updatedUser)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"UPDATE {Enum.GetName(DatabaseTables.Users)} SET DisplayName = @displayName, Email = @email, HashedPassword = @hashedPassword, PhoneNumber = @phoneNumber WHERE UserId = @userId";
            cmd.Parameters.AddWithValue("@displayName", updatedUser.DisplayName);
            cmd.Parameters.AddWithValue("@email", updatedUser.Email);
            cmd.Parameters.AddWithValue("@hashedPassword", updatedUser.HashedPassword);
            cmd.Parameters.AddWithValue("@phoneNumber", updatedUser.PhoneNumber);
            cmd.Parameters.AddWithValue("@userId", updatedUser.Id);
            await cmd.ExecuteNonQueryAsync();
        }



        /// <summary>
        /// Delete a user<br/>
        /// Must delete affected events first
        /// </summary>
        /// <param name="userId">Id of user to delete</param>
        public async Task DeleteUser(long userId)
        {
            using SQLiteTransaction transaciton = _sqlConnection.BeginTransaction();
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.Transaction = transaciton;

            try
            {
                cmd.Parameters.AddWithValue("@userId", userId);

                // Delete user connections
                cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.UserConnections)} WHERE FriendUserId = @userId OR UserId = @userId";
                await cmd.ExecuteNonQueryAsync();

                // Delete refresh tokens assocaited with user
                cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.RefreshTokens)} WHERE UserId = @userId";
                await cmd.ExecuteNonQueryAsync();

                // Delete user
                cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.Users)} WHERE UserId = @userId";
                await cmd.ExecuteNonQueryAsync();

                transaciton.Commit();
            }
            catch
            {
                transaciton.Rollback();
                throw;
            }

        }
        #endregion



        #region Friend
        /// <summary>
        /// Add a friend to the user
        /// </summary>
        /// <param name="userId">Id of user</param>
        /// <param name="otherUserId">Id of user to add as a friend</param>
        public async Task AddFriend(long userId, long otherUserId)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"UPDATE {Enum.GetName(DatabaseTables.UserConnections)} SET ConnectionType = @connectType WHERE (OtherUserId = @otherUserId AND UserId = @userId) OR (OtherUserId = @userId AND UserId = @otherUserId)";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@otherUserId", otherUserId);
            cmd.Parameters.AddWithValue("@connectType", Enum.GetName(UserConnectionTypes.Friends));
            await cmd.ExecuteNonQueryAsync();
        }



        /// <summary>
        /// Add friend request
        /// </summary>
        /// <param name="userIdSending">Id of user sending the friend request</param>
        /// <param name="userIdRecieving">Id of user receiving the friend request</param>
        /// <returns></returns>
        public async Task AddFriendRequest(long userIdSending, long userIdRecieving)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"INSERT INTO {Enum.GetName(DatabaseTables.UserConnections)} (UserId, OtherUserId, ConnectionType) VALUES (@userId, @otherUserId, @connectType)";
            cmd.Parameters.AddWithValue("@userId", userIdSending);
            cmd.Parameters.AddWithValue("@otherUserId", userIdRecieving);
            cmd.Parameters.AddWithValue("@connectType", Enum.GetName(UserConnectionTypes.SentFriendRequest));
            await cmd.ExecuteNonQueryAsync();
        }



        /// <summary>
        /// Add a friend from the user
        /// </summary>
        /// <param name="userId">Id of user</param>
        /// <param name="otherUserId">Id of user to remove as a friend</param>
        public async Task RemoveFriend(long userId, long otherUserId)
        {
            await RemoveUserConnectionEntry(userId, otherUserId, UserConnectionTypes.Friends);
        }



        /// <summary>
        /// Decline a friend request
        /// </summary>
        /// <param name="userId">Id of user</param>
        /// <param name="otherUserId">Id of user to decline a friend request from</param>
        public async Task DeclineFriendRequest(long userId, long otherUserId)
        {
            await RemoveUserConnectionEntry(userId, otherUserId, UserConnectionTypes.SentFriendRequest);
        }



        /// <summary>
        /// Remove an entry from the UserConnections table
        /// </summary>
        /// <param name="userId">Id of user</param>
        /// <param name="otherUserId">Id of user to decline a friend request from</param>
        /// <param name="connectionType">Connect type filter</param>
        private async Task RemoveUserConnectionEntry(long userId, long otherUserId, UserConnectionTypes connectionType)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.UserConnections)} WHERE ((OtherUserId = @otherUserId AND UserId = @userId) OR (OtherUserId = @userId AND UserId = @otherUserId)) AND ConnectionType = @connectType";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@otherUserId", otherUserId);
            cmd.Parameters.AddWithValue("@connectType", Enum.GetName(connectionType));
            await cmd.ExecuteNonQueryAsync();
        }
        #endregion
    }
}
