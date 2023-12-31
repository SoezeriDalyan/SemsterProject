using Npgsql;

namespace MonsterTradingCardGame
{
    internal interface IPostgresDB
    {

        public void InitilizeDB();

        public string AddUser(User user);
        public string CreateSession(User user);
        public string CreatePackandCards(List<Card> card, string token);
        public string CreatePack();
        public void CreateCards(List<Card> cards, string packUUID);
        public string BuyPack(string Token);
        public string GetPackUUID();
        public string GetAllCards(string token);
        public string GetDeck(string token);
    }

    internal class DB : IPostgresDB
    {
        //Todo: Timer that deletes old sessions or updates
        //Check money Later in buy packs

        private string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=password;Database=postgres";

        /// <summary>
        /// Create tables if they don't exist
        /// </summary>
        public void InitilizeDB()
        {
            using (var connection = new NpgsqlConnection(connectionString))//set connection
            {
                connection.Open();//open Connection
                var query = """
                    --drop table if exists deck;
                    --drop table if exists Card;
                    --drop table if exists Packs;
                    --drop table if exists usersession;
                    --drop table if exists users;

                    CREATE TABLE IF NOT EXISTS Users(
                        Username VARCHAR unique Primary Key,
                        Password VARCHAR,
                        VirtualCoins INTEGER,
                    	Image Varchar,
                    	Bio Varchar,
                        ELO INTEGER
                    );

                    CREATE TABLE IF NOT EXISTS UserSession(
                        SessionID serial,
                        Username VARCHAR PRIMARY KEY,
                        SessionToken Varchar,
                        LAST_ACTIVITY_TIME TIMESTAMP,
                        FOREIGN KEY (Username) REFERENCES Users(Username)
                    );
             
                    CREATE TABLE IF NOT EXISTS Packs(
                    PackID varchar primary key,
                    Used bool
                    );

                    CREATE TABLE IF NOT EXISTS Card(
                        CardID varchar Primary Key,
                        CardName VARCHAR,
                        Damage decimal,
                        Attribute Varchar,
                        Type Varchar,
                        Username varchar,
                        FOREIGN KEY (Username) REFERENCES Users(Username),
                        PackID varchar,
                        FOREIGN KEY (PackID) REFERENCES Packs(PackID)
                    );

                    CREATE TABLE IF NOT EXISTS Deck(
                        DeckID serial,
                        Username VARCHAR,
                    	CardID VARCHAR,
                    	FOREIGN KEY (CardID) REFERENCES Card(CardID),
                        FOREIGN KEY (Username) REFERENCES Users(Username),
                    	PRIMARY KEY (CardID, Username)
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
        /// <returns>String with Error or Success</returns>
        public string AddUser(User user)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "INSERT INTO Users (Username, Password, VirtualCoins, Image, Bio, ELO) VALUES (@Username, @Password, @VirtualCoins, @Image, @Bio, @ELO);";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    //Add and set parameters
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Password", user.Password);
                    command.Parameters.AddWithValue("@VirtualCoins", user.VirtualCoins);
                    command.Parameters.AddWithValue("@Image", "");
                    command.Parameters.AddWithValue("@Bio", "");
                    command.Parameters.AddWithValue("@ELO", 100);

                    using (NpgsqlCommand writer = new NpgsqlCommand(query, connection))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                            Console.WriteLine($"Created User: {user.Username}");
                            return $"Success: User {user.Username} created";
                        }
                        catch (Exception ex)
                        {
                            //Tells me directly that the username is duplicate because i set it as primary key (The code is the Error Code:
                            //The error with code 23505 is thrown when trying to insert a row that would violate a unique index or primary key.)
                            if (ex.Message.Contains("23505:"))
                            {
                                Console.WriteLine($"Error while creating User: {user.Username}");
                                return $"Error: Could not Create User {user.Username}, because User already exists";
                            }
                        }
                    }
                }

                connection.Close();
            }
            return $"Success: User {user.Username} created";
        }

        /// <summary>
        /// Create a Session, no check needed if existing, because when trying to insert the same username twice, it will crash => 23505
        /// But the crash is intentional, which is why I don't always have to check first whether the username already exists.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>String with Error or Success</returns>
        public string CreateSession(User user)
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
                            Console.WriteLine($"Created Session for User: {user.Username}");
                            return $"Success: {user.Username} logged in: {user.Username}-mtcgToken";
                        }
                        catch (Exception ex)
                        {
                            //Tells me directly that the session exists because i set the username as primary key (The code is the Error Code:
                            //The error with code 23505 is thrown when trying to insert a row that would violate a unique index or primary key.)
                            if (ex.Message.Contains("23505:"))
                            {
                                Console.WriteLine($"Error while creating Session for User: {user.Username}");
                                return "Error: Session already exists and is Valid";
                            }
                        }
                    }
                }

                connection.Close();
            }
            return "Error";
        }


        //Todo Check
        /// <summary>
        /// Checking if Session Exists => maybe own method for Check
        /// </summary>
        /// <param name="card"></param>
        /// <param name="token"></param>
        /// <returns> A string message </returns>
        public string CreatePackandCards(List<Card> cards, string token)
        {
            string packUUID = String.Empty;
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
                            packUUID = CreatePack();
                            //SaveCards
                            CreateCards(cards, packUUID);
                        }
                    }
                }

                connection.Close();
            }
            Console.WriteLine($"Created Pack: {packUUID}");
            return "Success: Pack created";
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

                string query = "INSERT INTO Packs (PackID, Used) VALUES (@PackID, false);";


                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PackID", pack.PackID);
                    command.ExecuteNonQuery();
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
            //todo => check if admintoken is used
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                cards.ForEach(card =>
                {
                    string query = " INSERT INTO Card(CardID, CardName, Damage, Attribute, Type, PackID) VALUES (@CardID, @CardName, @Damage, @Attribute, @Type, @PackID)";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CardID", card.ID);
                        command.Parameters.AddWithValue("@CardName", card.Name);
                        command.Parameters.AddWithValue("@Damage", card.Damage);
                        command.Parameters.AddWithValue("@Attribute", card.Attribute.ToString());
                        command.Parameters.AddWithValue("@Type", card.Type.ToString());
                        command.Parameters.AddWithValue("@PackID", packUUID);

                        command.ExecuteNonQuery();
                    }
                    Console.WriteLine($"Inserted Card: {card.ID}");
                });

                connection.Close();
            }
        }

        /// <summary>
        /// A User is buying a pack with VC (Virtual Coins)
        /// </summary>
        /// <param name="token"></param>
        /// <returns>A string Message</returns>
        public string BuyPack(string token)
        {
            string packid, username = "";
            //Get a pack,
            //Search for all cards of the packs in 
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
                            while (reader.Read())
                            {
                                username = reader.GetString(reader.GetOrdinal("username"));
                                Console.Write(username);
                                packid = GetPackUUID();
                                if (packid != String.Empty)
                                {
                                    string updatePackandCards = UpdatePackRelatedTransaction(packid, username);
                                    if (updatePackandCards.Contains("Error"))
                                    {
                                        Console.WriteLine(updatePackandCards);
                                        return updatePackandCards;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("No pack left");
                                    return "Error: No Pack left";
                                }

                            }
                        }
                    }
                }

                connection.Close();
            }

            return $"Success: {username} has bought a pack";
        }

        /// <summary>
        /// This method updates all nesessary things in the DB Such as
        /// the pack being used
        /// the cards, which belongs to a certain user now
        /// and the Coins of a user itself, because a pack costs 5 VirtualCoins
        /// </summary>
        /// <param name="packid"></param>
        /// <param name="username"></param>
        public string UpdatePackRelatedTransaction(string packid, string username)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                //Check if user has enough money
                if (!CheckIfUserHasEounghMoney(username, connection))
                {
                    return "Error: User has not enough money to purchase";
                }
                else
                {
                    //Update that pack is used
                    string query = "Update Packs Set used = true where packid = @PackID";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("PackID", packid);

                        command.ExecuteNonQuery();
                    }


                    //Update username in cards
                    query = "Update Card Set username = @Username where packid = @PackID";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("PackID", packid);
                        command.Parameters.AddWithValue("Username", username);
                        command.ExecuteNonQuery();
                    }


                    //Update User Coins
                    query = "Update Users Set virtualcoins = virtualcoins - 5 where username = @Username";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("PackID", packid);
                        command.Parameters.AddWithValue("Username", username);
                        command.ExecuteNonQuery();
                    }
                }

                connection.Close();
            }

            return "Success: User has enough money to purchase";
        }

        /// <summary>
        /// This searches for a pack in the DB that is unsed so the user can buy it
        /// </summary>
        /// <returns>the pack uuid which will be used</returns>
        public string GetPackUUID()
        {
            string packid = String.Empty;
            bool used = false;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "select * from Packs where used = false limit 1";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        //Check if i get Rows back => means that there is an active Session
                        if (reader.HasRows == true)
                        {

                            while (reader.Read())
                            {
                                packid = reader.GetString(reader.GetOrdinal("packid"));
                                used = reader.GetBoolean(reader.GetOrdinal("used"));

                                if (used)
                                {
                                    Console.WriteLine($" no Pack left to buy");
                                    return String.Empty;
                                }
                                else
                                {
                                    Console.WriteLine($" bought Pack: {packid}");
                                    return packid;
                                }
                            }
                        }
                    }
                }

                connection.Close();
            }

            return packid;
        }

        /// <summary>
        /// Get all Cards that bleonges to the user associated with the Token
        /// </summary>
        /// <param name="token"></param>
        /// <returns>All Cards with all stats</returns>
        public string GetAllCards(string token)
        {
            string username = String.Empty, allCardsReturn = String.Empty;
            Card? c = null;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                username = GetUserDataFromSession(token);

                string query = "select * from Card where username = @Username";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Username", username);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        //Check if i get Rows back => means that there is an active Session
                        if (reader.HasRows == true)
                        {
                            while (reader.Read())
                            {
                                username = reader.GetString(reader.GetOrdinal("username"));

                                c = new Card(reader.GetString(reader.GetOrdinal("cardid")), reader.GetString(reader.GetOrdinal("cardname")), reader.GetInt32(reader.GetOrdinal("damage")));
                                //Todo Json.tojson
                                allCardsReturn += c.ToString() + "\n";
                            }
                        }
                    }
                }

                connection.Close();
            }

            return $"Success: Here Are Your Cards: {allCardsReturn}";
        }

        /// <summary>
        /// Assembles the Deck
        /// </summary>
        /// <param name="token"></param>
        /// <param name="cards"></param>
        /// <returns>A string Message</returns>
        public string assembleDeck(string token, List<string> cards)
        {
            string username = String.Empty, allCardsReturn = String.Empty;


            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                username = GetUserDataFromSession(token);

                //GetDeck

                if (GetDeck(token) == String.Empty)
                {
                    string query = "Insert Into Deck (Username, CardID) Values (@Username, @CardID)";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("Username", username);

                        cards.ForEach(cardID =>
                        {
                            if (command.Parameters.Count > 1)
                            {
                                command.Parameters.Remove("CardID");
                            }
                            command.Parameters.AddWithValue("CardID", cardID);
                            command.ExecuteNonQuery();
                        });
                    }
                }
                else
                {
                    Console.WriteLine($"User {username} already has a Deck");
                    return "Error: Already has a deck";
                }

                connection.Close();
            }

            Console.WriteLine($"User {username} set Deck complete");
            return "Success: Deck is set";
        }

        public string GetDeck(string token)
        {
            List<string> cards = new List<string>();
            string username = String.Empty, allCardsReturn = String.Empty, cardID = String.Empty;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                username = GetUserDataFromSession(token);

                string query = "select * from Deck where username = @Username";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Username", username);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        //Check if i get Rows back => means that there is an active Session
                        if (reader.HasRows == true)
                        {
                            while (reader.Read())
                            {
                                username = reader.GetString(reader.GetOrdinal("username"));
                                cards.Add(reader.GetString(reader.GetOrdinal("cardid")));
                            }
                        }
                    }
                }

                GetCards(cards, token).ForEach(card =>
                {
                    allCardsReturn += $"{card.ID}, {card.Name}, {card.Damage}\n";
                });

                connection.Close();
            }
            Console.WriteLine($"User {username}'s Deck: \n{allCardsReturn}");
            return allCardsReturn;
        }

        public string GetUserData(string username, string token)
        {
            string usernameInSession = String.Empty, Image = String.Empty, Bio = String.Empty;
            int VirtualCoins = 0;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                usernameInSession = GetUserDataFromSession(token);

                if (usernameInSession == username)
                {
                    string query = "select * from Users where username = @Username";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("Username", username);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            //Check if i get Rows back => means that there is an active Session
                            if (reader.HasRows == true)
                            {
                                while (reader.Read())
                                {
                                    Image = reader.GetString(reader.GetOrdinal("image"));
                                    Bio = reader.GetString(reader.GetOrdinal("bio"));
                                    //Name????? Fragen Todo
                                    VirtualCoins = reader.GetInt32(reader.GetOrdinal("virtualcoins"));
                                }
                            }
                        }
                    }
                }
                else
                {
                    return "Error: Unauthorized";
                }
                connection.Close();
            }

            return $"Success: Data provided: Username {username}, VirtualCoins {VirtualCoins}, Image {Image}, Bio {Bio.Replace(" ", "_")}";
        }

        public string UpdateUserData(string username, string token, string image, string bio)
        {
            string usernameInSession = String.Empty, Image = String.Empty, Bio = String.Empty;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                usernameInSession = GetUserDataFromSession(token);

                if (usernameInSession == username)
                {
                    string query = "UPDATE Users SET image = @Image, bio = @Bio WHERE username = @Username";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("Username", username);
                        command.Parameters.AddWithValue("Image", image);
                        command.Parameters.AddWithValue("Bio", bio);

                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    return "Error: Unauthorized";
                }
                connection.Close();
            }

            return $"Success: User updated";
        }


        public List<Card> GetCards(List<string> cardIDs, string token)
        {
            List<Card> cards = new List<Card>();
            foreach (var id in cardIDs)
            {
                string username = String.Empty, allCardsReturn = String.Empty, cardID = String.Empty;
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "select * from Card where cardid = @CardId";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("CardId", id);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            //Check if i get Rows back => means that there is an active Session
                            if (reader.HasRows == true)
                            {
                                while (reader.Read())
                                {
                                    cards.Add(new Card(reader.GetString(reader.GetOrdinal("cardid")), reader.GetString(reader.GetOrdinal("cardname")), reader.GetDouble(reader.GetOrdinal("damage"))));
                                }
                            }
                        }
                    }

                    connection.Close();
                }
            }

            return cards;
        }

        public string GetUserDataFromSession(string token)
        {
            string username = String.Empty;
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
                            while (reader.Read())
                            {
                                username = reader.GetString(reader.GetOrdinal("username"));
                            }
                        }
                    }
                }

                connection.Close();
            }

            return username;
        }

        public bool CheckIfUserHasEounghMoney(string username, NpgsqlConnection connection)
        {
            int vc = 0;
            string query = "select * from Users where username = @Username";

            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("Username", username);

                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    //Check if i get Rows back => means that there is an active Session
                    if (reader.HasRows == true)
                    {
                        while (reader.Read())
                        {
                            vc = reader.GetInt32(reader.GetOrdinal("virtualcoins"));
                        }
                    }
                }
            }

            if (vc - 5 >= 0)
            {
                return true;
            }

            return false;
        }
    }
}
