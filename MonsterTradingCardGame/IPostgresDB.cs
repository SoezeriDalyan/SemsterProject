using Npgsql;

namespace MonsterTradingCardGame
{
    internal interface IPostgresDB
    {

        public void InitilizeDB();

        public bool AddUser(User user);
        public string CreateSession(User user);
        public void CreatePackandCards(List<Card> card, string token);
        public string CreatePack();
        public void CreateCards(List<Card> cards, string packUUID);
        public void BuyPack(string Token);
        public string GetPackUUID();
        public string GetAllCards(string token);
        public string GetDeck(string token);
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
                        VirtualCoins INTEGER,
                    	Image Varchar,
                    	Bio Varchar
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
                        --attribute Varchar,
                        --type TIMESTAMP,
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
                            return $"{user.Username}-mtcgToken";
                        }
                        catch (Exception ex)
                        {
                            //Tells me directly that the username is duülicate because i set it as primary key (The code is the Error Code:
                            //The error with code 23505 is thrown when trying to insert a row that would violate a unique index or primary key.)
                            if (ex.Message.Contains("23505:"))
                            {
                                return null;
                            }
                        }
                    }
                }

                connection.Close();
            }
            return null;
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

                string query = "INSERT INTO Packs (PackID, Used) VALUES (@PackID, false);";


                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PackID", pack.PackID);
                    command.ExecuteNonQuery();
                    //using (NpgsqlCommand writer = new NpgsqlCommand(query, connection))
                    //{
                    //    command.ExecuteNonQuery();
                    //}
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
                    string query = " INSERT INTO Card(CardID, CardName, Damage, PackID) VALUES (@CardID, @CardName, @Damage, @PackID)";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CardID", card.ID);
                        command.Parameters.AddWithValue("@CardName", card.Name);
                        command.Parameters.AddWithValue("@Damage", card.Damage);
                        command.Parameters.AddWithValue("@PackID", packUUID);

                        command.ExecuteNonQuery();
                        //using (NpgsqlCommand writer = new NpgsqlCommand(query, connection))
                        //{
                        //    command.ExecuteNonQuery();
                        //}
                    }
                    Console.WriteLine($"Inserted Card: {card.ID}");
                });

                connection.Close();
            }
        }

        //Todo: Timer that deletes old sessions or updates
        //Also that validate user is done before doing the next db manipulations >= nicht unbedingt
        //Exceptions and ... 
        // => Check if user has the crads that it adds to his deck


        /// <summary>
        /// A User is buying a pack with VC (Virtual Coins)
        /// </summary>
        /// <param name="token"></param>
        public void BuyPack(string token)
        {
            string packid;
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
                                string username = reader.GetString(reader.GetOrdinal("username"));
                                Console.WriteLine(username);
                                packid = GetPackUUID();
                                UpdatePackRelatedTransaction(packid, username);
                            }
                        }
                    }
                }

                connection.Close();
            }
        }

        /// <summary>
        /// This method updates all nesessary things in the DB Such as
        /// the pack being used
        /// the cards, which belongs to a certain user now
        /// and the Coins of a user itself, because a pack costs 5 VirtualCoins
        /// </summary>
        /// <param name="packid"></param>
        /// <param name="username"></param>
        public void UpdatePackRelatedTransaction(string packid, string username)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();


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

                connection.Close();
            }
        }

        /// <summary>
        /// This searches for a pack in the DB that is unsed so the user can buy it
        /// </summary>
        /// <returns>the pack uuid which will be used</returns>
        public string GetPackUUID()
        {
            string packid = String.Empty;
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

                                Console.WriteLine($"ID: {packid}");
                            }
                        }
                        //Add exception that no packs available
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
            Card c = null;
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

                query = "select * from Card where username = @Username";

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
                                //Console.WriteLine(c.ToString());
                                allCardsReturn += c.ToString() + "\n";
                            }
                        }
                    }
                }

                connection.Close();
            }

            return allCardsReturn;
        }

        /// <summary>
        /// Assembles the Deck
        /// </summary>
        /// <param name="token"></param>
        /// <param name="cards"></param>
        public void assembleDeck(string token, List<string> cards)
        {
            string username = String.Empty, allCardsReturn = String.Empty;


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

                //GetDeck

                if (GetDeck(token) == String.Empty)
                {
                    query = "Insert Into Deck (Username, CardID) Values (@Username, @CardID)";

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
                    Console.WriteLine("Already has a deck");
                    //ADD Exception
                }

                connection.Close();
            }
        }

        public string GetDeck(string token)
        {
            string username = String.Empty, allCardsReturn = String.Empty, cardID = String.Empty;
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

                query = "select * from Deck where username = @Username";

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
                                allCardsReturn += reader.GetString(reader.GetOrdinal("cardid")) + "\n";
                            }
                        }
                    }
                }

                connection.Close();
            }

            return allCardsReturn;
        }

        //Todo: method that just conirms the token!!!!!!!

        public string GetUserData(string username, string token)
        {
            string usernameInSession = String.Empty, Image = String.Empty, Bio = String.Empty;
            int VirtualCoins = 0;
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
                                usernameInSession = reader.GetString(reader.GetOrdinal("username"));
                            }
                        }
                    }
                }

                if (usernameInSession == username)
                {
                    query = "select * from Users where username = @Username";

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
                                    if (!reader.IsDBNull(3))
                                        Image = reader.GetString(reader.GetOrdinal("image"));
                                    else
                                        Image = "No image";
                                    if (!reader.IsDBNull(4))
                                        Bio = reader.GetString(reader.GetOrdinal("bio"));
                                    else
                                        Bio = "No Bio";

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

            return $"Username: {username}, VirtualCoins: {VirtualCoins}, Image: {Image}, Bio: {Bio}";
        }

        public string UpdateUserData(string username, string token, string image, string bio)
        {
            string usernameInSession = String.Empty, Image = String.Empty, Bio = String.Empty;
            int VirtualCoins = 0;
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
                                usernameInSession = reader.GetString(reader.GetOrdinal("username"));
                            }
                        }
                    }
                }

                if (usernameInSession == username)
                {
                    query = "UPDATE Users SET image = @Image, bio = @Bio WHERE username = @Username";

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

            return $"Updated User Succsessfuly";
        }
    }
}
