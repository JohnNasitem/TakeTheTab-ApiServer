//***********************************************************************************
//Program: IEventRepository.cs
//Description: Interface for the event repository
//Date: Sep 25, 2025
//Author: John Nasitem
//***********************************************************************************



using takethetab_server.Application.Dtos.Activities.CreateActivity;
using takethetab_server.Domain.Entities;

namespace takethetab_server.Application.Interfaces
{
    public interface IEventRepository
    {
        #region Event
        /// <summary>
        /// Create an event
        /// </summary>
        /// <param name="eventName">Name of event</param>
        /// <param name="eventDate">Date of event</param>
        /// <param name="creator">User who created the event</param>
        /// <param name="participants">Users participating in the event</param>
        /// <returns><see cref="Event"/> instance if successull created, otherwise null</returns>
        public Task<Event?> CreateEvent(string eventName, DateTime eventDate, User creator, List<User> participants);



        /// <summary>
        /// Delete an event 
        /// </summary>
        /// <param name="eventId">Id of event to delete</param>
        public Task DeleteEvent(long eventId);



        /// <summary>
        /// Update event with a new name
        /// </summary>
        /// <param name="eventId">Id of event to update</param>
        /// <param name="newName">New name of event</param>
        public Task<bool> UpdateEvent(long eventId, string newName, DateTime newDate, List<User> newParticipants, List<long> participantsToRemove);



        /// <summary>
        /// Get events from the database
        /// </summary>
        /// <returns>List of events</returns>
        public Task<List<Event>> GetEvents();
        #endregion



        #region Activity
        /// <summary>
        /// Create an activity under an event
        /// </summary>
        /// <param name="activityName">Name of activity</param>
        /// <param name="isGratuityTypePercent">Is the gratuity type percent</param>
        /// <param name="gratuityAmount">Gratuity amount</param>
        /// <param name="addFivePercentTax">Should a 5% tax be included in the cost</param>
        /// <param name="parentEvent">Event the activity is being added to</param>
        /// <param name="payee">User who paid for the event</param>
        /// <param name="items">Items in the activity</param>
        /// <returns><see cref="Activity"/> instance</returns>
        public Task<Activity?> CreateActivity(string activityName, bool isGratuityTypePercent, decimal gratuityAmount, bool addFivePercentTax, Event parentEvent, User payee, List<CreateActivityItem> items);



        /// <summary>
        /// Update an activity
        /// </summarynewName
        /// <param name="activity">Activity</param>
        /// <param name="newName">Name of activity</param>
        /// <param name="isGratuityTypePercent">Is the gratuity type percent</param>
        /// <param name="gratuityAmount">Gratuity amount</param>
        /// <param name="addFivePercentTax">Should a 5% tax be included in the cost</param>
        /// <param name="itemsToAdd">Items to add to the activity</param>
        /// <param name="itemsToRemove">Item ids to remove from the activity</param>
        /// <returns>List of new activity items if updating the activity was successfully, otherwise null</returns>
        public Task<List<ActivityItem>?> UpdateActivity(Activity activity, string newName, bool isGratuityTypePercent, decimal gratuityAmount, bool addFivePercentTax, List<CreateActivityItem> itemsToAdd, List<long> itemsToRemove);



        /// <summary>
        /// Delete an activity
        /// </summary>
        /// <param name="activityId">Id of activity being deleted</param>
        public Task DeleteActivity(long activityId);
        #endregion
    }
}
