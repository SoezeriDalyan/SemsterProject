using Npgsql;

namespace MonsterTradingCardGame
{
    internal interface IPostgresDB
    {

        public void InitilizeDB();

        public bool AddUser(User user);
        public bool CreateSession(User user);
        public void CreatePackandCards(List<Card> card, string token);
        public string CreatePack();
        public void CreateCards(List<Card> cards, string packUUID);
    }

    internal class DB : IPostgresDB
    {
        private string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=password;Database=postgres";

        /// <summary>
        /// Create tables if they don't exist
        /// </summary>
        /// <returns>Nothing, it waits for the creation</returns>
        public void InitilizeDB()
        {
            using (var connection = new NpgsqlConnection(connectionString))//set connection
            {
                connection.Open();//open Connection
                var query = """
                    CREATE TABLE IF NOT EXISTS Users(
                        Username VARCHAR unique Primary Key,
                        Password VARCHAR,
                        VirtualCoins INTEGER
                    );

                    CREATE TABLE IF NOT EXISTS UserSession(
                        SessionID serial,
                        Username VARCHAR PRIMARY KEY,
                        SessionToken Varchar,
                        LAST_ACTIVITY_TIME TIMESTAMP,
                        FOREIGN KEY (Username) REFERENCES Users(Username)
                    );
             
                    CREATE TABLE IF NOT EXISTS Packs(
                    PackID varchar primary key
                    );

                    CREATE TABLE IF NOT EXISTS Card(
                        CardID varchar Primary Key,
                        CardName VARCHAR,
                        Damage decimal,
                        --attribute Varchar,
                        --type TIMESTAMP,
                        PackID varchar,
                        FOREIGN KEY (PackID) REFERENCES Packs(PackID)
                    );
                    """;

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Close the connection
                connection.Close();
            }
            Console.WriteLine("Done with the Tables");
        }

        /// <summary>
        /// Add a User, no check needed no check needed if existing, because when trying to insert the same username twice, it will crash => 23505
        /// But the crash is intentional, which is why I don't always have to check first whether the username already exists.
        /// </summary>
        /// <param name="user"> This is an object of the Class User</param>
        /// <returns>Bool (Creation Succsessful: true | Not Successful: false)</returns>
        public bool AddUser(User user)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "INSERT INTO Users (Username, Password, VirtualCoins) VALUES (@Username, @Password, @VirtualCoins);";


                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    //Add and set parameters
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Password", user.Password);
                    command.Parameters.AddWithValue("@VirtualCoins", user.VirtualCoins);

                    using (NpgsqlCommand writer = new NpgsqlCommand(query, connection))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            //Tells me directly that the username is duülicate because i set it as primary key (The code is the Error Code:
                            //The error with code 23505 is thrown when trying to insert a row that would violate a unique index or primary key.)
                            if (ex.Message.Contains("23505:"))
                            {
                                return false;
                            }
                        }
                    }
                }

                connection.Close();
            }
            return true;
        }

        /// <summary>
        /// Create a Session, no check needed if existing, because when trying to insert the same username twice, it will crash => 23505
        /// But the crash is intentional, which is why I don't always have to check first whether the username already exists.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Bool (Creation Succsessful: true | Not Successful: false)</returns>
        public bool CreateSession(User user)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "INSERT INTO UserSession (Username, SessionToken, LAST_ACTIVITY_TIME) VALUES (@Username, @Token, CURRENT_TIMESTAMP);";


                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Token", $"{user.Username}-mtcgToken");

                    using (NpgsqlCommand writer = new NpgsqlCommand(query, connection))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            //Tells me directly that the username is duülicate because i set it as primary key (The code is the Error Code:
                            //The error with code 23505 is thrown when trying to insert a row that would violate a unique index or primary key.)
                            if (ex.Message.Contains("23505:"))
                            {
                                return false;
                            }
                        }
                    }
                }

                connection.Close();
            }
            return true;
        }

        /// <summary>
        /// Checking if Session Exists => maybe own method for Check
        /// </summary>
        /// <param name="card"></param>
        /// <param name="token"></param>
        public void CreatePackandCards(List<Card> cards, string token)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "select * from Usersession where SessionToken = @Token";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Token", token.Replace("Bearer ", ""));

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        //Check if i get Rows back => means that there is an active Session
                        if (reader.HasRows == true)
                        {
                            //SavePack
                            string packUUID = CreatePack();
                            //SaveCards
                            CreateCards(cards, packUUID);
                        }
                    }
                }

                connection.Close();
            }
        }

        /// <summary>
        /// Creates a new pack with an ID => this ID will be recorded in the Card Table
        /// </summary>
        /// <returns>A UUID of the new Created Pack</returns>
        public string CreatePack()
        {
            Pack pack = new Pack();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "INSERT INTO Packs (PackID) VALUES (@PackID);";


                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PackID", pack.PackID);

                    using (NpgsqlCommand writer = new NpgsqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                connection.Close();
            }

            return pack.PackID;
        }

        /// <summary>
        /// Saves the Cards in the DB
        /// </summary>
        /// <param name="cards"></param>
        /// <param name="packUUID"></param>
        public void CreateCards(List<Card> cards, string packUUID)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                cards.ForEach(card =>
                {
                    string query = " INSERT INTO Card(CardID, CardName, Damage, PackID) VALUES (@CardID, @CardName, @Damage, @PackID)";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CardID", card.ID);
                        command.Parameters.AddWithValue("@CardName", card.Name);
                        command.Parameters.AddWithValue("@Damage", card.Damage);
                        command.Parameters.AddWithValue("@PackID", packUUID);

                        using (NpgsqlCommand writer = new NpgsqlCommand(query, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    Console.WriteLine($"Inserted Card: {card.ID}");
                });

                connection.Close();
            }
        }


    }
}
