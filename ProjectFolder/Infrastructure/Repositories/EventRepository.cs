//***********************************************************************************
//Program: EventRepository.cs
//Description: Repository for events and activities
//Date: Sep 25, 2025
//Author: John Nasitem
//***********************************************************************************



using System.Data.SQLite;
using takethetab_server.Application;
using takethetab_server.Application.Dtos.Activities.CreateActivity;
using takethetab_server.Application.Interfaces;
using takethetab_server.Domain.Entities;
using takethetab_server.Infrastructure.Database;
using static takethetab_server.Infrastructure.Database.DatabaseInitializer;



namespace takethetab_server.Infrastructure.Repositories
{
    public class EventRepository(DatabaseInitializer dbInit, UserService userService) : IEventRepository
    {
        // Connection to database
        private readonly SQLiteConnection _sqlConnection = dbInit.SqlConnection;



        #region Event
        /// <summary>
        /// Create an event
        /// </summary>
        /// <param name="eventName">Name of event</param>
        /// <param name="eventDate">Date of event</param>
        /// <param name="creator">User who created the event</param>
        /// <param name="participants">Users participating in the event</param>
        /// <returns><see cref="Event"/> instance if successull created, otherwise null</returns>
        public async Task<Event?> CreateEvent(string eventName, DateTime eventDate, User creator, List<User> participants)
        {
            using SQLiteTransaction transaction = _sqlConnection.BeginTransaction();
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.Transaction = transaction;

            try
            {
                // Add event to the database
                cmd.CommandText = $@"INSERT INTO {Enum.GetName(DatabaseTables.Events)} (CreatorUserId, EventName, EventDate) VALUES (@creatorUserId, @eventName, @eventDate)";
                cmd.Parameters.AddWithValue("@eventName", eventName);
                cmd.Parameters.AddWithValue("@eventDate", eventDate.ToString("o"));
                cmd.Parameters.AddWithValue("@creatorUserId", creator.Id);
                await cmd.ExecuteNonQueryAsync();

                // Get events's id
                cmd.CommandText = "SELECT last_insert_rowid();";
                long eventId = Convert.ToInt64(await cmd.ExecuteScalarAsync());

                // Add participants to the EventParticipants table
                foreach (User participant in participants)
                    await AddEventParticipant(eventId, participant.Id, transaction);

                await transaction.CommitAsync();

                return new Event(eventId, creator, eventName, eventDate, [], participants);
            }
            catch
            {
                await transaction.RollbackAsync();
                return null;
            }
        }



        /// <summary>
        /// Add a user as a event participant
        /// </summary>
        /// <param name="eventId">Id of event to add to</param>
        /// <param name="participantId">Id of user participanting in event</param>
        /// <param name="hasPaid">Has the participant paid for the event</param>
        /// <param name="transaction">Encompassing transaction if it exists</param>
        private async Task AddEventParticipant(long eventId, long participantId, SQLiteTransaction? transaction = null)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();

            if (transaction != null)
                cmd.Transaction = transaction;

            cmd.CommandText = $@"INSERT INTO {Enum.GetName(DatabaseTables.EventParticipants)} (EventId, UserId) VALUES (@eventId, @userId)";
            cmd.Parameters.AddWithValue("@eventId", eventId);
            cmd.Parameters.AddWithValue("@userId", participantId);
            await cmd.ExecuteNonQueryAsync();
        }



        /// <summary>
        /// Remove a user as a event participant
        /// </summary>
        /// <param name="eventId">Id of event to remove from</param>
        /// <param name="participantId">Id of user participanting no longer in event</param>
        /// <param name="transaction">Encompassing transaction if it exists</param>
        private async Task RemoveEventParticipant(long eventId, long participantId, SQLiteTransaction? transaction = null)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();

            if (transaction != null)
                cmd.Transaction = transaction;

            cmd.CommandText = $@"DELETE FROM {Enum.GetName(DatabaseTables.EventParticipants)} WHERE EventId = @eventId AND UserId = @userId";
            cmd.Parameters.AddWithValue("@eventId", eventId);
            cmd.Parameters.AddWithValue("@userId", participantId);
            await cmd.ExecuteNonQueryAsync();
        }



        /// <summary>
        /// Delete an event 
        /// </summary>
        /// <param name="eventId">Id of event to delete</param>
        public async Task DeleteEvent(long eventId)
        {
            using SQLiteTransaction transaction = _sqlConnection.BeginTransaction();
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.Transaction = transaction;

            try
            {
                cmd.Parameters.AddWithValue("@eventId", eventId);

                // Delete activities relate to event
                cmd.CommandText = $"SELECT ActivityId FROM {Enum.GetName(DatabaseTables.Activities)} WHERE EventId = @eventId;";
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    await DeleteActivity(reader.GetInt64(0));

                // Delete entries in Event participants table related to this event
                cmd.CommandText = $"DELETE from {Enum.GetName(DatabaseTables.EventParticipants)} WHERE EventId = @eventId";
                await cmd.ExecuteNonQueryAsync();

                // Delete event
                cmd.CommandText = $"DELETE from {Enum.GetName(DatabaseTables.Events)} WHERE EventId = @eventId";
                await cmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }



        /// <summary>
        /// Update event with new details
        /// </summary>
        /// <param name="eventId">Id of event to update</param>
        /// <param name="newName">New name of event</param>
        /// <param name="newDate">New date of event</param>
        /// <param name="newParticipants">Users to add as participants</param>
        /// <param name="participantsToRemove">Users to remove as participants</param>
        /// <returns>True if event was successfully updated, otherwise false</returns>
        public async Task<bool> UpdateEvent(long eventId, string newName, DateTime newDate, List<User> newParticipants, List<long> participantsToRemove)
        {
            using SQLiteTransaction transaction = _sqlConnection.BeginTransaction();
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.Transaction = transaction;

            try
            {
                // Update details
                cmd.CommandText = $@"UPDATE {Enum.GetName(DatabaseTables.Events)} SET EventName = @eventName, EventDate = @eventDate WHERE EventId = @eventId";
                cmd.Parameters.AddWithValue("@eventName", newName);
                cmd.Parameters.AddWithValue("@eventDate", newDate.ToString("o"));
                cmd.Parameters.AddWithValue("@eventId", eventId);
                await cmd.ExecuteNonQueryAsync();

                // Update participants
                foreach (User participant in newParticipants)
                    await AddEventParticipant(eventId, participant.Id, transaction);

                foreach (long participantId in participantsToRemove)
                    await RemoveEventParticipant(eventId, participantId, transaction);

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }



        /// <summary>
        /// Get events from the database
        /// </summary>
        /// <returns>List of events</returns>
        public async Task<List<Event>> GetEvents()
        {
            List<Event> events = [];

            Dictionary<long, User> usersDict = userService.Users.ToDictionary(u => u.Id, u => u);

            using (SQLiteCommand cmd = _sqlConnection.CreateCommand())
            {
                // Get events
                cmd.CommandText = $"SELECT * FROM {Enum.GetName(DatabaseTables.Events)}";

                Dictionary<long, Event> eventsDict = [];

                using var eventsReader = await cmd.ExecuteReaderAsync();
                while (await eventsReader.ReadAsync())
                {
                    Event e = new(
                        id: long.Parse(eventsReader["EventId"].ToString()!),
                        creator: usersDict[long.Parse(eventsReader["CreatorUserId"].ToString()!)],
                        name: eventsReader["EventName"].ToString()!,
                        date: DateTime.Parse(eventsReader["EventDate"].ToString()!, null, System.Globalization.DateTimeStyles.RoundtripKind),
                        [],
                        []
                    );

                    events.Add(e);
                    eventsDict.Add(e.Id, e);
                }

                await eventsReader.CloseAsync();

                // Populate Event's Participants property
                cmd.CommandText = $"SELECT * FROM {Enum.GetName(DatabaseTables.EventParticipants)}";
                using var eventParticipantsReader = await cmd.ExecuteReaderAsync();
                while (await eventParticipantsReader.ReadAsync())
                {
                    eventsDict[long.Parse(eventParticipantsReader["EventId"].ToString()!)]
                        .Participants
                        .Add(usersDict[long.Parse(eventParticipantsReader["UserId"].ToString()!)]);
                }

                await eventParticipantsReader.CloseAsync();

                Dictionary<long, Activity> activitiesDict = [];

                // Populate Event's Activities property 
                cmd.CommandText = $"SELECT * FROM {Enum.GetName(DatabaseTables.Activities)}";
                using var activitiesReader = await cmd.ExecuteReaderAsync();
                while (await activitiesReader.ReadAsync())
                {
                    Activity activity = new(
                        id: long.Parse(activitiesReader["ActivityId"].ToString()!),
                        name: activitiesReader["ActivityName"].ToString()!,
                        isGratuityTypePercent: bool.Parse(activitiesReader["IsGratuityTypePercent"].ToString()!),
                        gratuityAmount: decimal.Parse(activitiesReader["GratuityAmount"].ToString()!),
                        addFivePercentTax: bool.Parse(activitiesReader["AddFivePercentTax"].ToString()!),
                        parentEvent: eventsDict[long.Parse(activitiesReader["EventId"].ToString()!)],
                        payee: usersDict[long.Parse(activitiesReader["PayeeUserId"].ToString()!)],
                        items: []
                    );

                    activity.ParentEvent.Activities.Add(activity);
                    activitiesDict.Add(activity.Id, activity);
                }

                await activitiesReader.CloseAsync();
                Dictionary<long, ActivityItem> itemsDict = [];

                // Populate Activity's items property
                cmd.CommandText = $"SELECT * FROM {Enum.GetName(DatabaseTables.ActivityItems)}";
                using var itemsReader = await cmd.ExecuteReaderAsync();
                while (await itemsReader.ReadAsync())
                {
                    ActivityItem item = new(
                        long.Parse(itemsReader["ItemId"].ToString()!),
                        activitiesDict[long.Parse(itemsReader["ActivityId"].ToString()!)],
                        itemsReader["ItemName"].ToString()!,
                        decimal.Parse(itemsReader["ItemCost"].ToString()!),
                        bool.Parse(itemsReader["IsSplitEvenly"].ToString()!),
                        []
                    );

                    activitiesDict[long.Parse(itemsReader["ActivityId"].ToString()!)].Items.Add(item);
                    itemsDict.Add(item.Id, item);
                }

                await itemsReader.CloseAsync();

                // Populate item's payer property
                cmd.CommandText = $"SELECT * FROM {Enum.GetName(DatabaseTables.PayerToActivityItem)}";
                using var itemsPayerReader = await cmd.ExecuteReaderAsync();
                while (await itemsPayerReader.ReadAsync())
                {
                    itemsDict[long.Parse(itemsPayerReader["ItemId"].ToString()!)]
                        .Payers
                        .Add(new ActivityItemPayer()
                        {
                            Payer = usersDict[long.Parse(itemsPayerReader["UserId"].ToString()!)],
                            AmountOwing = decimal.Parse(itemsPayerReader["PayerOwing"].ToString()!),
                            HasPaid = bool.Parse(itemsPayerReader["HasPaid"].ToString()!),
                            PaymentConfirmed = bool.Parse(itemsPayerReader["PaymentConfirmed"].ToString()!)
                        });
                }
            }

            return events;
        }
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
        public async Task<Activity?> CreateActivity(string activityName, bool isGratuityTypePercent, decimal gratuityAmount, bool addFivePercentTax, Event parentEvent, User payee, List<CreateActivityItem> items)
        {
            using SQLiteTransaction transaction = _sqlConnection.BeginTransaction();
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.Transaction = transaction;

            try
            {
                // Add activity to the database
                cmd.CommandText = $@"INSERT INTO {Enum.GetName(DatabaseTables.Activities)} (ActivityName, IsGratuityTypePercent, GratuityAmount, AddFivePercentTax, EventId, PayeeUserId) VALUES (@activityName, @isGratuityTypePercent, @gratuityAmount, @addFivePercentTax, @eventId, @payeeId)";
                cmd.Parameters.AddWithValue("@activityName", activityName);
                cmd.Parameters.AddWithValue("@isGratuityTypePercent", isGratuityTypePercent);
                cmd.Parameters.AddWithValue("@gratuityAmount", gratuityAmount);
                cmd.Parameters.AddWithValue("@addFivePercentTax", addFivePercentTax);
                cmd.Parameters.AddWithValue("@eventId", parentEvent.Id);
                cmd.Parameters.AddWithValue("@payeeId", payee.Id);
                await cmd.ExecuteNonQueryAsync();

                // Get acitivity's id
                cmd.CommandText = "SELECT last_insert_rowid();";
                long activityId = Convert.ToInt64(await cmd.ExecuteScalarAsync());

                Activity newActivity = new Activity(activityId, activityName, isGratuityTypePercent, gratuityAmount, addFivePercentTax, parentEvent, payee, []);
                List<ActivityItem> newItems = [];

                Dictionary<long, User> usersDict = userService.Users.ToDictionary(u => u.Id, u => u);

                // Populate children tables
                foreach (CreateActivityItem item in items)
                    newActivity.Items.Add(
                        await AddActivityItem(
                            newActivity,
                            item.ItemName,
                            item.ItemCost,
                            item.IsSplitTypeEvenly,
                            item.Payers.Select(p => new ActivityItemPayer()
                            {
                                Payer = usersDict[p.Key],
                                AmountOwing = p.Value,
                                PaymentConfirmed = false,
                                HasPaid = false,
                            }).ToList(),
                            transaction));

                await transaction.CommitAsync();
                return newActivity;
            }
            catch
            {
                await transaction.RollbackAsync();
                return null;
            }
        }



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
        public async Task<List<ActivityItem>?> UpdateActivity(Activity activity, string newName, bool isGratuityTypePercent, decimal gratuityAmount, bool addFivePercentTax, List<CreateActivityItem> itemsToAdd, List<long> itemsToRemove)
        {
            using SQLiteTransaction transaction = _sqlConnection.BeginTransaction();
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.Transaction = transaction;

            try
            {
                cmd.CommandText = $@"UPDATE {Enum.GetName(DatabaseTables.Activities)} SET ActivityName = @activityName, IsGratuityTypePercent = @isGratuityTypePercent, GratuityAmount = @gratuityAmount, AddFivePercentTax = @addFivePercentTax WHERE ActivityId = @activityId";
                cmd.Parameters.AddWithValue("@activityId", activity.Id);
                cmd.Parameters.AddWithValue("@activityName", newName);
                cmd.Parameters.AddWithValue("@isGratuityTypePercent", isGratuityTypePercent);
                cmd.Parameters.AddWithValue("@gratuityAmount", gratuityAmount);
                cmd.Parameters.AddWithValue("@addFivePercentTax", addFivePercentTax);
                await cmd.ExecuteNonQueryAsync();

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@activityId", activity.Id);

                foreach (long itemId in itemsToRemove)
                    await DeleteActivityItem(itemId, transaction);

                Dictionary<long, User> usersDict = userService.Users.ToDictionary(u => u.Id, u => u);
                List<ActivityItem> newItems = [];

                // Add new items
                foreach (CreateActivityItem item in itemsToAdd)
                    newItems.Add(
                        await AddActivityItem(
                            activity,
                            item.ItemName,
                            item.ItemCost,
                            item.IsSplitTypeEvenly,
                            item.Payers.Select(p => new ActivityItemPayer()
                            {
                                Payer = usersDict[p.Key],
                                AmountOwing = p.Value,
                                PaymentConfirmed = false,
                                HasPaid = false,
                            }).ToList(),
                            transaction)
                        );

                await transaction.CommitAsync();
                return newItems;
            }
            catch
            {
                await transaction.RollbackAsync();
                return null;
            }

        }



        /// <summary>
        /// Delete an activity
        /// </summary>
        /// <param name="activityId">Id of activity being deleted</param>
        public async Task DeleteActivity(long activityId)
        {
            using SQLiteTransaction transaction = _sqlConnection.BeginTransaction();
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.Transaction = transaction;

            try
            {
                cmd.Parameters.AddWithValue("@activityId", activityId);

                // Delete entries in payers to activity item table related to this activity
                cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.PayerToActivityItem)} WHERE ActivityId = @activityId";
                await cmd.ExecuteNonQueryAsync();

                // Delete entries in activity items table related to this activity
                cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.ActivityItems)} WHERE ActivityId = @activityId";
                await cmd.ExecuteNonQueryAsync();

                // Delete entries in activities table related to this event
                cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.Activities)} WHERE ActivityId = @activityId";
                await cmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        #endregion



        #region ActivityItem
        /// <summary>
        /// Add an item to an activity
        /// </summary>
        /// <param name="activity">Activity to add the item to</param>
        /// <param name="itemName">Name of item</param>
        /// <param name="itemCost">Cost of item</param>
        /// <param name="isSplitEvenly">Is the item cost split evenly between the payers</param>
        /// <param name="transaction">Encompassing transaction if it exists</param>
        /// <returns>Id of newly added item</returns>
        private async Task<ActivityItem> AddActivityItem(Activity activity, string itemName, decimal itemCost, bool isSplitEvenly, List<ActivityItemPayer> itemPayers, SQLiteTransaction? transaction = null)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();

            if (transaction != null)
                cmd.Transaction = transaction;

            cmd.CommandText = $@"INSERT INTO {Enum.GetName(DatabaseTables.ActivityItems)} (ActivityId, ItemName, ItemCost, IsSplitEvenly) VALUES (@acitivityId, @itemName, @itemCost, @isSplitEvenly)";
            cmd.Parameters.AddWithValue("@acitivityId", activity.Id);
            cmd.Parameters.AddWithValue("@itemName", itemName);
            cmd.Parameters.AddWithValue("@itemCost", itemCost);
            cmd.Parameters.AddWithValue("@isSplitEvenly", isSplitEvenly);
            await cmd.ExecuteNonQueryAsync();

            // Get acitivity's id
            cmd.CommandText = "SELECT last_insert_rowid();";
            long itemId = Convert.ToInt64(await cmd.ExecuteScalarAsync());

            foreach (ActivityItemPayer payer in itemPayers)
            {
                cmd.Parameters.Clear();

                cmd.CommandText = $@"INSERT INTO {Enum.GetName(DatabaseTables.PayerToActivityItem)} (ItemId, UserId, PayerOwing, HasPaid, PaymentConfirmed) VALUES (@itemId, @payerUserId, @payerOwing, @hasPaid, @paymentConfirmed)";
                cmd.Parameters.AddWithValue("@itemId", itemId);
                cmd.Parameters.AddWithValue("@payerUserId", payer.Payer.Id);
                cmd.Parameters.AddWithValue("@payerOwing", payer.AmountOwing);
                cmd.Parameters.AddWithValue("@hasPaid", payer.HasPaid);
                cmd.Parameters.AddWithValue("@paymentConfirmed", payer.PaymentConfirmed);
                await cmd.ExecuteNonQueryAsync();
            }

            return new ActivityItem(itemId, activity, itemName, itemCost, isSplitEvenly, itemPayers);
        }



        /// <summary>
        /// Delete an item from its parent activity
        /// </summary>
        /// <param name="itemId">Id of item to delete</param>
        public async Task DeleteActivityItem(long itemId, SQLiteTransaction? transaction = null)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();

            if (transaction != null)
                cmd.Transaction = transaction;

            cmd.Parameters.AddWithValue("@itemId", itemId);

            // Delete entries in payers to activity item table related to the item
            cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.PayerToActivityItem)} WHERE ItemId = @itemId";
            await cmd.ExecuteNonQueryAsync();

            // Delete entries the activity item
            cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.ActivityItems)} WHERE ItemId = @itemId";
            await cmd.ExecuteNonQueryAsync();
        }
        #endregion
    }
}
