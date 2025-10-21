//***********************************************************************************
//Program: EventService.cs
//Description: Handles event actions
//Date: Sep 25, 2025
//Author: John Nasitem
//***********************************************************************************



using takethetab_server.Application.Dtos.Activities.CreateActivity;
using takethetab_server.Application.Interfaces;
using takethetab_server.Domain.Comparers;
using takethetab_server.Domain.Entities;

namespace takethetab_server.Application
{
    public class EventService
    {
        public List<Event> Events { get; set; } = null!;



        private readonly IEventRepository _eventRepository;



        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;

            GetDatabaseData();
        }



        /// <summary>
        /// Populate global vars with data from the database
        /// </summary>
        public async void GetDatabaseData()
        {
            Events = await _eventRepository.GetEvents();
        }



        #region Events
        /// <summary>
        /// Get events related to the specified user id
        /// </summary>
        /// <param name="userId">Id of user to get related events from</param>
        /// <returns>Events</returns>
        public List<Event> GetUserEvents(long userId)
        {
            HashSet<Event> events = [];

            foreach (Event e in Events)
                if (e.Creator.Id == userId || e.Participants.Any(p => p.Id == userId))
                    events.Add(e);

            return [.. events];
        }



        /// <summary>
        /// Fetch an event
        /// </summary>
        /// <param name="eventId">Id of event</param>
        /// <returns><see cref="Event"/> Instance</returns>
        public Event? FetchEvent(long eventId)
        {
            return Events.Find(e => e.Id == eventId);
        }



        /// <summary>
        /// Create an event
        /// </summary>
        /// <param name="eventName">Name of event</param>
        /// <param name="eventCreator">User who created event</param>
        /// <param name="participants">Ids of users participanting in the event</param>
        /// <returns>Event id if event was successfully created, otherwise null</returns>
        public async Task<long?> CreateEvent(string eventName, DateTime eventDate, User eventCreator, List<User> participants)
        {
            try
            {
                Event? newEvent = await _eventRepository.CreateEvent(eventName, eventDate, eventCreator, participants);

                if (newEvent == null)
                    return null;

                Events.Add(newEvent);
                return newEvent.Id;
            }
            catch
            {
                return null;
            }
        }



        /// <summary>
        /// Edit an events
        /// </summary>
        /// <param name="eventToUpdate">Id of event being editted</param>
        /// <param name="eventName">New event name</param>
        /// <param name="eventDate">New event date</param>
        /// <param name="newParticipants">New event participants</param>
        /// <returns>True if edit was successful, otherwise false</returns>
        public async Task<bool> UpdateEvent(Event eventToUpdate, string eventName, DateTime eventDate, List<User> newParticipants)
        {
            try
            {
                List<User> newParticipantStateClone = new(newParticipants);
                List<long> participantsToRemove = [];
                Dictionary<long, User> newParticipantDict = newParticipants.ToDictionary(u => u.Id, u => u);

                foreach (var currentParticipant in eventToUpdate.Participants)
                {
                    // Check if the current participant exists in the new participants
                    if (!newParticipantDict.TryGetValue(currentParticipant.Id, out var newParticipantState))
                        // If the new participant state doesnt have the participant, mark them to be removed
                        participantsToRemove.Add(currentParticipant.Id);

                    // Remove from the list after processing so that the remaining items are participants that need to be added
                    newParticipants.Remove(currentParticipant);
                }

                if (await _eventRepository.UpdateEvent(eventToUpdate.Id, eventName, eventDate, newParticipants, participantsToRemove))
                {
                    eventToUpdate.Name = eventName;
                    eventToUpdate.Date = eventDate;
                    eventToUpdate.Participants = newParticipantStateClone;
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }



        /// <summary>
        /// Delete event
        /// </summary>
        /// <param name="eventToDelete">event being deleted</param>
        /// <returns>True if event was successfully deleted, otherwise false</returns>
        public async Task<bool> DeleteEvent(Event eventToDelete)
        {
            try
            {
                await _eventRepository.DeleteEvent(eventToDelete.Id);
                Events.Remove(eventToDelete);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion



        #region Actities
        /// <summary>
        /// Fetch an activity
        /// </summary>
        /// <param name="eventId">Id of event</param>
        /// <param name="eventId">Id of activity</param>
        /// <returns><see cref="Activity"/> Instance</returns>
        public Activity? FetchActivity(long eventId, long activityId)
        {
            return Events.Find(e => e.Id == eventId)?.Activities.Find(a => a.Id == activityId);
        }



        /// <summary>
        /// Create an activity
        /// </summary>
        /// <param name="activityName">Name of activity</param>
        /// <param name="isGratuityAmountPercent">Should the gratuity amount be percent</param>
        /// <param name="gratuityAmount">Gratuity amount</param>
        /// <param name="addFivePercentTax">Should a 5% tax be added to the tax</param>
        /// <param name="parentEvent">Event the activity belongs under</param>
        /// <param name="payee">User who paid for activity</param>
        /// <param name="items">User who paid for activity</param>
        /// <returns>Activity id if activity was successfully created, otherwise null</returns>
        public async Task<long?> CreateActivity(string activityName, bool isGratuityAmountPercent, decimal gratuityAmount, bool addFivePercentTax, Event parentEvent, User payee, List<CreateActivityItem> items)
        {
            try
            {
                Activity? activity = await _eventRepository.CreateActivity(activityName, isGratuityAmountPercent, gratuityAmount, addFivePercentTax, parentEvent, payee, items);

                if (activity == null)
                    return null;

                parentEvent.Activities.Add(activity);
                return activity.Id;
            }
            catch
            {
                return null;
            }
        }




        /// <summary>
        /// Delete an activity
        /// </summary>
        /// <param name="parentEvent">Parent event the activity belongs to</param>
        /// <param name="activity">Activity being deleted</param>
        /// <returns>True if activity was successfully deleted, otherwise false</returns>
        public async Task<bool> DeleteActivity(Event parentEvent, Activity activity)
        {
            try
            {
                await _eventRepository.DeleteActivity(activity.Id);
                parentEvent.Activities.Remove(activity);
                return true;
            }
            catch
            {
                return false;
            }
        }



        /// <summary>
        /// Update an activity
        /// </summary>
        /// <param name="newName">Name of activity</param>
        /// <param name="isGratuityAmountPercent">Should the gratuity amount be percent</param>
        /// <param name="gratuityAmount">Gratuity amount</param>
        /// <param name="addFivePercentTax">Should a 5% tax be added to the tax</param>
        /// <param name="items">User who paid for activity</param>
        /// <returns>True if activity was successfully updated, otherwise false</returns>
        public async Task<bool> UpdateActivity(Activity activity, string newName, bool isGratuityAmountPercent, decimal gratuityAmount, bool addFivePercentTax, List<CreateActivityItem> items)
        {
            try
            {
                CreateActivityItemComparer comparer = new();

                // Covert existing items to create activity item
                HashSet<CreateActivityItem> existingItemSet = new(
                    activity.Items.Select(i => new CreateActivityItem
                    {
                        ItemName = i.Name,
                        ItemCost = i.Cost,
                        IsSplitTypeEvenly = i.IsSplitTypeEvenly,
                        Payers = i.Payers.ToDictionary(p => p.Payer.Id, p => p.AmountOwing)
                    }),
                    comparer
                );

                // Find which new items are missing (to add)
                List<CreateActivityItem> itemsToAdd = items
                    .Where(i => !existingItemSet.Contains(i))
                    .ToList();

                // HashSet of new items
                HashSet<CreateActivityItem> newItemSet = new(items, comparer);

                // Find which existing items no longer exist (to remove)
                HashSet<long> itemsToRemove = activity.Items
                    .Where(i => !newItemSet.Contains(new CreateActivityItem
                    {
                        ItemName = i.Name,
                        ItemCost = i.Cost,
                        IsSplitTypeEvenly = i.IsSplitTypeEvenly,
                        Payers = i.Payers.ToDictionary(p => p.Payer.Id, p => p.AmountOwing)
                    }))
                    .Select(i => i.Id)
                    .ToHashSet();

                List<ActivityItem>? newItems = await _eventRepository.UpdateActivity(activity, newName, isGratuityAmountPercent, gratuityAmount, addFivePercentTax, itemsToAdd, [.. itemsToRemove]);

                if (newItems != null)
                {
                    activity.Name = newName;
                    activity.AddFivePercentTax = addFivePercentTax;
                    activity.GratuityAmount = gratuityAmount;
                    activity.IsGratuityTypePercent = isGratuityAmountPercent;
                    activity.Items.RemoveAll(i => itemsToRemove.Contains(i.Id));
                    activity.Items.AddRange(newItems);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}
