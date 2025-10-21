//***********************************************************************************
//Program: DatabaseInitializer.cs
//Description: Handles database initialization
//Date: Sep 25, 2025
//Author: John Nasitem
//***********************************************************************************



using System.Data.SQLite;



namespace takethetab_server.Infrastructure.Database
{
    public class DatabaseInitializer
    {
        // Connection to database
        public SQLiteConnection SqlConnection { get; }



        public DatabaseInitializer()
        {
            SqlConnection = new SQLiteConnection(@"Data Source=takethetab.db");
            SqlConnection.Open();

            // Enable foreign keys
            using SQLiteCommand cmd = SqlConnection.CreateCommand();
            cmd.CommandText = "PRAGMA foreign_keys = ON;";
            cmd.ExecuteNonQuery();

            InitializeDatabase();
        }



        /// <summary>
        /// Intialize database if they dont exist
        /// </summary>
        public void InitializeDatabase()
        {
            if (HasDatabaseBeenInitialized())
                return;

            using SQLiteCommand cmd = SqlConnection.CreateCommand();

            // Initialize Users table
            cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS {Enum.GetName(DatabaseTables.Users)}(
                                        UserId INTEGER PRIMARY KEY AUTOINCREMENT, 
                                        DisplayName TEXT NOT NULL,
                                        Email TEXT NOT NULL,
                                        HashedPassword TEXT NOT NULL,
                                        PhoneNumber TEXT
                                        )";
            cmd.ExecuteNonQuery();

            // Initialize UserConnections table
            cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS {Enum.GetName(DatabaseTables.UserConnections)}(
                                        UserId INTEGER,
                                        OtherUserId INTEGER,
                                        ConnectionType INTEGER,
                                        FOREIGN KEY (UserId) REFERENCES {Enum.GetName(DatabaseTables.Users)}(UserId)
                                        FOREIGN KEY (OtherUserId) REFERENCES {Enum.GetName(DatabaseTables.Users)}(UserId)
                                        PRIMARY KEY (UserId, OtherUserId)
                                        )";
            cmd.ExecuteNonQuery();

            // Initialize Events table
            cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS {Enum.GetName(DatabaseTables.Events)}(
                                        EventId INTEGER PRIMARY KEY AUTOINCREMENT, 
                                        CreatorUserId INTEGER NOT NULL,
                                        EventName TEXT NOT NULL,
                                        EventDate TEXT NOT NULL,
                                        FOREIGN KEY (CreatorUserId) REFERENCES {Enum.GetName(DatabaseTables.Users)}(UserId)
                                        )";
            cmd.ExecuteNonQuery();

            // Initialize EventParticipants table
            cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS {Enum.GetName(DatabaseTables.EventParticipants)}(
                                        EventId INTEGER NOT NULL, 
                                        UserId INTEGER NOT NULL,
                                        FOREIGN KEY (EventId) REFERENCES {Enum.GetName(DatabaseTables.Events)}(EventId)
                                        FOREIGN KEY (UserId) REFERENCES {Enum.GetName(DatabaseTables.Users)}(UserId)
                                        PRIMARY KEY (EventId, UserId)
                                        )";
            cmd.ExecuteNonQuery();

            // Initialize Activities table
            cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS {Enum.GetName(DatabaseTables.Activities)}(
                                        ActivityId INTEGER PRIMARY KEY AUTOINCREMENT, 
                                        ActivityName TEXT NOT NULL,
                                        IsGratuityTypePercent BOOLEAN NOT NULL,
                                        GratuityAmount NUMERIC NOT NULL,
                                        AddFivePercentTax BOOLEAN NOT NULL,
                                        EventId INTEGER NOT NULL,
                                        PayeeUserId INTEGER NOT NULL,
                                        FOREIGN KEY (EventId) REFERENCES {Enum.GetName(DatabaseTables.Events)}(EventId)
                                        FOREIGN KEY (PayeeUserId) REFERENCES {Enum.GetName(DatabaseTables.Users)}(UserId)
                                        )";
            cmd.ExecuteNonQuery();

            // Initialize ActivityItems table
            cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS {Enum.GetName(DatabaseTables.ActivityItems)}(
                                        ItemId INTEGER PRIMARY KEY AUTOINCREMENT,
                                        ActivityId INTEGER NOT NULL,
                                        ItemName TEXT NOT NULL,
                                        ItemCost NUMERIC NOT NULL,
                                        IsSplitEvenly BOOLEAN NOT NULL,
                                        FOREIGN KEY (ActivityId) REFERENCES {Enum.GetName(DatabaseTables.Activities)}(ActivityId)
                                        )";
            cmd.ExecuteNonQuery();

            // Initialize PayerToActivityItem table
            cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS {Enum.GetName(DatabaseTables.PayerToActivityItem)}(
                                        ItemId INTEGER NOT NULL,
                                        UserId INTEGER NOT NULL,
                                        PayerOwing NUMERIC NOT NULL,
                                        HasPaid BOOLEAN NOT NULL,
                                        PaymentConfirmed BOOLEAN NOT NULL,
                                        FOREIGN KEY (ItemId) REFERENCES {Enum.GetName(DatabaseTables.ActivityItems)}(ItemId),
                                        FOREIGN KEY (UserId) REFERENCES {Enum.GetName(DatabaseTables.Users)}(UserId),
                                        PRIMARY KEY (ItemId, UserId)
                                        )";
            cmd.ExecuteNonQuery();

            // Initialize RefreshTokens table
            cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS {Enum.GetName(DatabaseTables.RefreshTokens)}(
                                        Token TEXT NOT NULL PRIMARY KEY,
                                        UserId INTEGER NOT NULL,
                                        BrowserId TEXT NOT NULL,
                                        CreationDate DATETIME NOT NULL,
                                        ExpiryDate DATETIME NOT NULL,
                                        FOREIGN KEY (UserId) REFERENCES {Enum.GetName(DatabaseTables.Users)}(UserId)
                                        )";
            cmd.ExecuteNonQuery();
        }



        /// <summary>
        /// Checks if the database has initialized already
        /// </summary>
        /// <returns>true if the database is initialized, otherwise false</returns>
        private bool HasDatabaseBeenInitialized()
        {
            using SQLiteCommand cmd = SqlConnection.CreateCommand();
            cmd.CommandText = @"SELECT name
                                FROM sqlite_master
                                WHERE type='table' AND name NOT LIKE 'sqlite_%';";
            using SQLiteDataReader reader = cmd.ExecuteReader();
            return reader.HasRows;
        }



        public enum DatabaseTables
        {
            Users,
            UserConnections,
            Events,
            EventParticipants,
            Activities,
            ActivityItems,
            PayerToActivityItem,
            RefreshTokens
        }



        public enum UserConnectionTypes
        {
            Friends,
            SentFriendRequest,
        }
    }
}
