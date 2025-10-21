//***********************************************************************************
//Program: UserService.cs
//Description: Handles account actions
//Date: Sep 25, 2025
//Author: John Nasitem
//***********************************************************************************



using takethetab_server.Application.Interfaces;
using takethetab_server.Domain.Entities;

namespace takethetab_server.Application
{
    public class UserService
    {
        public List<User> Users { get; set; } = null!;



        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordHasher;


        public UserService(IUserRepository userRepo, IPasswordService passwordHasher)
        {
            _userRepository = userRepo;
            _passwordHasher = passwordHasher;

            GetDatabaseData();
        }



        /// <summary>
        /// Populate global vars with data from the database
        /// </summary>
        public async void GetDatabaseData()
        {
            Users = await _userRepository.GetUsers();
        }



        /// <summary>
        /// Verify user log in credentials
        /// </summary>
        /// <param name="email">Email of user</param>
        /// <param name="password">Password of user</param>
        /// <returns>True if credentials are valid, otherwise false</returns>
        public long? VerifyUserLoginCredentials(string email, string password)
        {
            User? userLoggingIn = Users.Find(u => u.Email.Equals(email, StringComparison.CurrentCultureIgnoreCase));

            if (userLoggingIn == null)
                return null;

            return _passwordHasher.VerifyPassword(password, userLoggingIn.HashedPassword) ? userLoggingIn.Id : null;
        }



        #region User
        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="email">Display name of new user</param>
        /// <param name="email">Email of new user</param>
        /// <param name="password">Password of new user</param>
        /// <param name="phoneNumber"></param>
        /// <returns>True if user was successfully added, false if user with email already exists</returns>
        public async Task<bool> CreateUser(string displayName, string email, string password, string? phoneNumber)
        {
            // If a user with the email already exists then don't create a new one
            if (Users.Any(u => u.Email.Equals(email, StringComparison.CurrentCultureIgnoreCase)))
                return false;

            Users.Add(await _userRepository.CreateUser(displayName, email, _passwordHasher.HashPassword(password), phoneNumber));

            return true;
        }



        /// <summary>
        /// Edits the specified user's profile
        /// </summary>
        /// <param name="userId">Id of user being editted</param>
        /// <param name="newDisplayName">New display name</param>
        /// <param name="newEmail">New email</param>
        /// <returns>True if user was successfully editted, otherwise false</returns>
        public async Task<bool> EditUserProfile(long userId, string newDisplayName, string newEmail)
        {
            User? user = Users.Find(u => u.Id == userId);

            if (user == null)
                return false;

            string oldName = user.DisplayName;
            string oldEmail = user.Email;

            try
            {
                user.Email = newEmail;
                user.DisplayName = newDisplayName;
                await _userRepository.UpdateUser(user);
                return true;
            }
            catch
            {
                user.Email = oldEmail;
                user.DisplayName = oldName;
                await _userRepository.UpdateUser(user);
                return false;
            }
        }



        /// <summary>
        /// Edits the specified user's password
        /// </summary>
        /// <param name="userId">Id of user being editted</param>
        /// <param name="oldPassword">User's old password</param>
        /// <param name="newPassword">User's new password</param>
        /// <returns>True if password was successfully editted, otherwise false</returns>
        public async Task<EditUserPasswordResponse> EditUserPassword(long userId, string oldPassword, string newPassword)
        {
            User? user = Users.Find(u => u.Id == userId);

            if (user == null)
                return EditUserPasswordResponse.UserNotFound;
            if (!_passwordHasher.VerifyPassword(oldPassword, user.HashedPassword))
                return EditUserPasswordResponse.OldPasswordIncorrect;

            string oldHashedPassword = user.HashedPassword;

            try
            {
                user.HashedPassword = _passwordHasher.HashPassword(newPassword);
                await _userRepository.UpdateUser(user);
                return EditUserPasswordResponse.Success;
            }
            catch
            {
                user.HashedPassword = oldHashedPassword;
                await _userRepository.UpdateUser(user);
                return EditUserPasswordResponse.ServerError;
            }
        }
        #endregion



        #region Friends
        /// <summary>
        /// Send user friend request
        /// </summary>
        /// <param name="userSending">Current user</param>
        /// <param name="userRecieving">User friend request is coming from</param>
        /// <returns><see cref="SendFriendRequestReponse"></returns>
        public async Task<SendFriendRequestReponse> SendFriendRequest(User userSending, User userRecieving)
        {
            if (userRecieving.OutgoingFriendRequests.Any(u => u.Id == userSending.Id))
                return await RespondToFriendRequest(userRecieving, userSending, true) ? SendFriendRequestReponse.AccpetedFriendRequest : SendFriendRequestReponse.FailedToAcceptFriendRequest;

            if (!userSending.OutgoingFriendRequests.Any(u => u.Id == userRecieving.Id))
                userSending.OutgoingFriendRequests.Add(userRecieving);
            if (!userRecieving.IncomingFriendRequests.Any(u => u.Id == userSending.Id))
                userRecieving.IncomingFriendRequests.Add(userSending);

            try
            {
                await _userRepository.AddFriendRequest(userSending.Id, userRecieving.Id);
                return SendFriendRequestReponse.SentRequest;
            }
            catch
            {
                return SendFriendRequestReponse.FailedToSentRequest;
            }
        }




        /// <summary>
        /// Accept user friend request
        /// </summary>
        /// <param name="userSending">Current user</param>
        /// <param name="userRecieving">User friend request is coming from</param>
        /// <param name="acceptedRequest">Did user accept the friend request</param>
        /// <returns>True if accepting the friend was successful, otherwise false</returns>
        public async Task<bool> RespondToFriendRequest(User userSending, User userRecieving, bool acceptedRequest)
        {
            //try
            //{
            userSending.OutgoingFriendRequests.RemoveAll(u => u.Id == userRecieving.Id);
            userSending.IncomingFriendRequests.RemoveAll(u => u.Id == userRecieving.Id);
            userRecieving.OutgoingFriendRequests.RemoveAll(u => u.Id == userSending.Id);
            userRecieving.IncomingFriendRequests.RemoveAll(u => u.Id == userSending.Id);

            if (acceptedRequest)
            {
                if (!userSending.Friends.Any(u => u.Id == userRecieving.Id))
                    userSending.Friends.Add(userRecieving);
                if (!userRecieving.Friends.Any(u => u.Id == userSending.Id))
                    userRecieving.Friends.Add(userSending);

                await _userRepository.AddFriend(userSending.Id, userRecieving.Id);
            }
            else
                await _userRepository.DeclineFriendRequest(userSending.Id, userRecieving.Id);

            return true;
            //}
            //catch
            //{
            //    return false;
            //}
        }



        /// <summary>
        /// Accept other user as a friend from user
        /// </summary>
        /// <param name="user">Current user</param>
        /// <param name="otherUser">User friend request is coming from</param>
        /// <returns>True if friend removal was successful</returns>
        public async Task<bool> RemoveFriend(User user, User otherUser)
        {
            try
            {
                user.Friends.RemoveAll(u => u.Id == otherUser.Id);
                otherUser.Friends.RemoveAll(u => u.Id == user.Id);
                await _userRepository.RemoveFriend(user.Id, otherUser.Id);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion




        public enum EditUserPasswordResponse
        {
            Success,
            UserNotFound,
            OldPasswordIncorrect,
            ServerError
        }



        public enum SendFriendRequestReponse
        {
            SentRequest,
            FailedToSentRequest,
            AccpetedFriendRequest,
            FailedToAcceptFriendRequest
        }
    }
}
