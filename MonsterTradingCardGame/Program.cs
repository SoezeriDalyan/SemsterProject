using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MonsterTradingCardGame;
using Newtonsoft.Json;
using Npgsql;

class Program
{
    static List<User> users = new List<User>();
    static List<Session> sessions = new List<Session>();
    static Pack pack = new Pack();
    static async Task Main()
    {
        await InitilizeDB();
        //Initilaize DB
        const int port = 10001;
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Listening on Address localhost...");
        Console.WriteLine($"Listening on port {port}...");
        string responseKontrukt = "HTTP/1.1 200 OK\r\nContent-Type: text/application/json\r\n\r\n";
        string responseData = String.Empty;
        byte[] responseBytes = null;
        while (true)
        {
            using (var client = await listener.AcceptTcpClientAsync())
            using (var networkStream = client.GetStream())//Create Stream, to send and recieve Data
            {
                var requestBytes = new byte[1024]; //Set byte length, TCP cuts Packest at 1024 bytes
                await networkStream.ReadAsync(requestBytes, 0, requestBytes.Length);
                var request = Encoding.UTF8.GetString(requestBytes);
                string body = GetRequestBody(request);
               
                //heandle user createn or login request
                if (request.StartsWith("POST /users HTTP/1.1"))
                {
                    //POST User, save User into (later) Postgres | ado.net
                    if (HandleUserCreation(body))
                    {
                        responseData = $"{responseKontrukt}Created Succsesfully";
                    }
                    else
                    {
                        responseData = $"{responseKontrukt}Creation Failed, Username exists already";
                        
                    }
                }
                else if (request.StartsWith("POST /sessions HTTP/1.1"))
                {
                    //POST User, save User into (later) Postgres | ado.net
                    if (HandleUserLogin(body))
                    {
                        responseData = $"{responseKontrukt}Login Succsesfully";
                    }
                    else
                    {
                        responseData = $"{responseKontrukt}Login Failed";
                    }
                }
                else if (request.StartsWith("POST /packages HTTP/1.1"))
                {
                    //ADd  --header "Authorization: Bearer admin-mtcgToken"
                    if (HandlePackCreation(body))
                    {
                        responseData = $"{responseKontrukt}Pack Created Succsesfully";
                    }
                }

                Console.WriteLine(responseData);
                responseBytes = Encoding.UTF8.GetBytes(responseData);
                await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
            }
        }
    }

    static bool HandleUserCreation(string body)
    {
        User user = JsonConvert.DeserializeObject<User>(body);
       
        //Check if there is a User with the same Username
        if (users.Find(x => x.Username == user.Username) == null)
        {
            users.Add(user);
            return true;
        }
        else
        return false;
    }

    static bool HandleUserLogin(string body)
    {
        User user = JsonConvert.DeserializeObject<User>(body);

        //Check if there is a User with the same Username
        if (sessions.Find(x => x.User.Username == user.Username) == null && users.Find(x => x.Username == user.Username) != null)
        {
            sessions.Add(new Session(user, true));
            return true;
        }
        else
            return false;
    }

    static bool HandlePackCreation(string body)
    {
        pack.Add(JsonConvert.DeserializeObject<List<Card>>(body));
        //pack.ForEach(x => Console.WriteLine($"{x.ID} {x.Name} {x.Damage}"));

        

        ////Check if there is a User with the same Username
        //if (sessions.Find(x => x.User.Username == user.Username) == null && users.Find(x => x.Username == user.Username) != null)
        //{
        //    sessions.Add(new Session(user, true));
        //    return true;
        //}
        //else
        //    return false;

        return true;
    }

    static string GetRequestBody(string request)
    {
        string[] requestLines = request.Split('\n');

        for (int i = 1; i < requestLines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(requestLines[i]))
            {
                // Headers end, and the body begins
                int bodyStartIndex = i + 1;
                return requestLines.Length > bodyStartIndex ? requestLines[bodyStartIndex].Trim() : "";
            }
        }

        return "";
    }

    static async Task InitilizeDB() 
    {
        // create tables if they don't exist
        var connectionString = "Host=localhost;Port=5432;Username=postgres;Password=password;Database=postgres";
        using( var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            var query = """
                CREATE TABLE IF NOT EXISTS Users(
                    Username VARCHAR,
                    Password VARCHAR,
                    VirtualCoins INTEGER
                    );
            """;

           // string query = "SELECT * FROM your_table WHERE column1 = @value";
            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
            {
                // Create and set parameters
                //NpgsqlParameter parameter = new NpgsqlParameter("@value", NpgsqlTypes.NpgsqlDbType.Varchar);
                //parameter.Value = "some_value";
                //command.Parameters.Add(parameter);

                using (NpgsqlCommand writer = new NpgsqlCommand(query, connection))
                {
                    //while (reader.Read())
                    //{
                    //    Console.WriteLine($"{reader["column1"]} - {reader["column2"]}");
                    //}
                    command.ExecuteNonQuery();
                }
            }

            // Close the connection
            connection.Close();
        }
    }
}
