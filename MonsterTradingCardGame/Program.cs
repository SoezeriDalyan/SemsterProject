using MonsterTradingCardGame;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static Pack pack = new Pack();
    static DB dB = new DB();
    private static readonly object lockObject = new object();

    static async Task Main()
    {
        Console.WriteLine("Createing Tables in the Postgres database if needed");
        //Initilaize DB
        dB.InitilizeDB();

        const int port = 10001;
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Listening on Address http://localhost:{port}...");

        string responseKontrukt = "HTTP/1.1 200 OK\r\nContent-Type: text/application/json\r\n\r\n";
        string responseData = String.Empty;
        byte[] responseBytes = null;

        while (true)
        {
            using (var client = await listener.AcceptTcpClientAsync())//Accept Connection from Clients
            using (var networkStream = client.GetStream())//Create Stream to client, to send and recieve Data
            {
                var requestBytes = new byte[1024]; //Set byte length, TCP cuts Packest at 1024 bytes
                await networkStream.ReadAsync(requestBytes, 0, requestBytes.Length);
                var request = Encoding.UTF8.GetString(requestBytes);

                string body = GetRequestBody(request);

                //implement improvements and correct the response => Json type
                //heandle user createn or login request
                if (request.StartsWith("POST /users HTTP/1.1"))
                {
                    if (HandleUserCreation(body))
                    {
                        //Successful
                        responseData = $"{responseKontrukt}Created Succsesfully";
                    }
                    else
                    {
                        //Not Successful
                        responseData = $"{responseKontrukt}Creation Failed, Username exists already";
                    }
                }
                else if (request.StartsWith("POST /sessions HTTP/1.1"))
                {
                    if (HandleUserLogin(body) != null)
                    {
                        //Successful
                        responseData = $"{responseKontrukt}Login Succsesfully";
                    }
                    else
                    {
                        //Not Successful
                        responseData = $"{responseKontrukt}Login Failed";
                    }
                }
                else if (request.StartsWith("POST /packages HTTP/1.1"))
                {
                    string headerToken = GetHeaderToken(request);
                    if (headerToken == String.Empty)
                    {
                        responseData = $"{responseKontrukt}Unauthorized";
                    }
                    else if (HandlePackCreation(body, headerToken))
                    {
                        responseData = $"{responseKontrukt}Pack Created Succsesfully";
                    }
                }
                else if (request.StartsWith("POST /transactions/packages HTTP/1.1"))
                {
                    string headerToken = GetHeaderToken(request);
                    if (headerToken == String.Empty)
                    {
                        responseData = $"{responseKontrukt}Unauthorized";
                    }
                    else if (BuysPack(headerToken))
                    {
                        responseData = $"{responseKontrukt}Transaction Successful";
                    }
                }
                else if (request.StartsWith("GET /cards HTTP/1.1"))
                {
                    string headerToken = GetHeaderToken(request);
                    string allCards = GetCards(headerToken);
                    if (headerToken == String.Empty)
                    {
                        responseData = $"{responseKontrukt}Unauthorized";
                    }
                    else if (allCards != String.Empty)
                    {
                        responseData = $"{responseKontrukt} All Cards: {allCards}";
                    }
                }
                else if (request.StartsWith("PUT /deck HTTP/1.1"))
                {
                    string headerToken = GetHeaderToken(request);
                    string allCards = assembleDeck(headerToken, body);
                    if (headerToken == String.Empty)
                    {
                        responseData = $"{responseKontrukt}Unauthorized";
                    }
                    else if (allCards != String.Empty)
                    {
                        responseData = $"{responseKontrukt} All Cards: {allCards}";
                    }
                }
                else if (request.StartsWith("GET /deck HTTP/1.1"))
                {
                    string headerToken = GetHeaderToken(request);
                    string assembledDeck = getDeck(headerToken);
                    if (headerToken == String.Empty)
                    {
                        responseData = $"{responseKontrukt}Unauthorized";
                    }
                    else if (assembledDeck != String.Empty)
                    {
                        responseData = $"{responseKontrukt} Deck: {assembledDeck}";
                    }
                }
                else if (request.StartsWith("GET /users"))
                {
                    string headerToken = GetHeaderToken(request);
                    string username = GetUsernameFromRequestUrl(request);

                    if (headerToken == String.Empty)
                    {
                        responseData = $"{responseKontrukt}Unauthorized";
                    }
                    else if (username != String.Empty)
                    {

                        responseData = $"{responseKontrukt}: {GetUserData(username, headerToken)}";
                    }
                }
                else if (request.StartsWith("PUT /users"))
                {
                    string headerToken = GetHeaderToken(request);
                    string username = GetUsernameFromRequestUrl(request);

                    if (headerToken == String.Empty)
                    {
                        responseData = $"{responseKontrukt}Unauthorized";
                    }
                    else if (username != String.Empty)
                    {

                        responseData = $"{responseKontrukt}: {UpdateUserData(username, headerToken, body)}";
                    }
                }
                else
                {
                    TcpClient client2 = null;
                    NetworkStream networkStream2 = null;
                    string headerToken = GetHeaderToken(request);
                    string headerToken2 = "";

                    responseBytes = Encoding.UTF8.GetBytes(responseKontrukt + "You are Palyer 1, and currently waiting in the Lobby");
                    networkStream.Write(responseBytes, 0, responseBytes.Length);

                    Task secondPlayerTask = Task.Run(async () =>
                    {
                        client2 = await listener.AcceptTcpClientAsync();
                        Console.WriteLine("Second player connected");

                        networkStream2 = client2.GetStream();

                        var requestBytes2 = new byte[1024];
                        await networkStream2.ReadAsync(requestBytes, 0, requestBytes.Length);
                        var request2 = Encoding.UTF8.GetString(requestBytes);

                        headerToken2 = GetHeaderToken(request2);

                        //Verify Token and get username

                        var responseBytes2 = Encoding.UTF8.GetBytes(responseKontrukt + "You are Palyer 2, and currently waiting in the Lobby");
                        networkStream2.Write(responseBytes2, 0, responseBytes2.Length);

                    });

                    await secondPlayerTask;

                    var p1Deck = dB.GetCards(getDeck(headerToken).Split("\n").ToList<string>(), headerToken);
                    var p2Deck = dB.GetCards(getDeck(headerToken2).Split("\n").ToList<string>(), headerToken2);


                    Battle b = new Battle(dB, new List<Player> { new Player(client, headerToken, p1Deck), new Player(client2, headerToken2, p2Deck) });

                    responseData = $"{responseKontrukt}:";
                }

                /*
                
                */





                Console.WriteLine(responseData);
                responseBytes = Encoding.UTF8.GetBytes(responseData);
                networkStream.Write(responseBytes, 0, responseBytes.Length);
            }
        }
    }


    /// <summary>
    /// I Deserialize the Json object into User object and pass it to the Method which handles the Database communication;
    /// </summary>
    /// <param name="body"></param>
    /// <returns>A bool, which indicates if the creation was succesfull or not (true = yes | false = no)</returns>
    static bool HandleUserCreation(string body)
    {
        User user = JsonConvert.DeserializeObject<User>(body);
        return dB.AddUser(user);
    }

    /// <summary>
    /// I Deserialize the Json object into User object and pass it to the Method which handles the Database communication;
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    static string HandleUserLogin(string body)
    {
        User user = JsonConvert.DeserializeObject<User>(body);
        return dB.CreateSession(user);
    }

    static bool HandlePackCreation(string body, string headerToken)
    {

        //JsonConvert.DeserializeObject<List<Card>>(body).ForEach(x => Console.WriteLine($"{x.ID} {x.Name} {x.Damage}"));

        dB.CreatePackandCards(JsonConvert.DeserializeObject<List<Card>>(body), headerToken);

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

    /// <summary>
    /// Saves and updates everything in the database
    /// </summary>
    /// <param name="headerToken"></param>
    /// <returns></returns>
    static bool BuysPack(string headerToken)
    {
        dB.BuyPack(headerToken);
        return true;
    }

    /// <summary>
    /// Receives all Cards the User has
    /// </summary>
    /// <param name="headerToken"></param>
    /// <returns>string of all Cardss</returns>
    static string GetCards(string headerToken)
    {
        return dB.GetAllCards(headerToken);
    }

    //Not Done
    static string assembleDeck(string headerToken, string body)
    {
        //noch verbessern
        List<string> cardIDs = JsonConvert.DeserializeObject<List<string>>(body);
        //check that cardsid should be 4 otherwise error
        if (cardIDs.Count < 4)
        {
            return "not enough cards selected";
        }
        else
        {
            dB.assembleDeck(headerToken, cardIDs);
            return body;
        }
    }

    static string getDeck(string headerToken)
    {
        return dB.GetDeck(headerToken);
    }

    static string GetUserData(string username, string headertoken)
    {
        return dB.GetUserData(username, headertoken);
    }

    static string UpdateUserData(string username, string headertoken, string body)
    {
        User user = JsonConvert.DeserializeObject<User>(body);
        return dB.UpdateUserData(username, headertoken, user.Image, user.Bio);
    }

    /// <summary>
    /// Extracts the Token from the Header
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Example: Authorization: Bearer Token; Output = Bearer Token</returns>
    static string GetHeaderToken(string request)
    {
        string[] lines = request.Split('\n');
        foreach (string line in lines)
        {
            if (line.Contains(":") && line.Contains("Authorization"))
            {
                string[] header = line.Split(':');
                string headerValue = header[1].Trim();
                return headerValue;
            }
        }

        return String.Empty;
    }

    /// <summary>
    /// Get the Body Data of the Request.
    /// The Body contains the Date in a Json format
    /// </summary>
    /// <param name="request"></param>
    /// <returns>The Json Data as String</returns>
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

    /// <summary>
    /// Because of the form of the request: GET http://localhost:10001/users/username
    /// This methodes gets the username inside the URL
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <returns>Username</returns>
    static string GetUsernameFromRequestUrl(string requestUrl)
    {
        // Assuming the format is "/users/{username}"
        string[] segments = requestUrl.Split('/');
        if (segments.Length >= 3 && segments[1] == "users")
        {
            return segments[2].Split(" ")[0];
        }
        else
        {
            return "Unknown";
        }
    }
}


