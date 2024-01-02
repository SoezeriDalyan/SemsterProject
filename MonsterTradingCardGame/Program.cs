using MonsterTradingCardGame;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static DB dB = new DB();
    public delegate void RouteDelegate();

    static async Task Main()
    {
        bool restartAll = false;
        if (restartAll)
        {
            dB.deleteDB();
        }
        Console.WriteLine("Createing Tables in the Postgres database if needed");

        //Initilaize DB
        dB.InitilizeDB();

        //set variables
        const int port = 10001;
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start(); //Start server
        Console.WriteLine($"Listening on Address http://localhost:{port}...");

        string responseKontrukt = "HTTP/1.1 200 OK\r\nContent-Type: text/application/json\r\n\r\n"; //needed for signaling players that they are waiting in a lobby
        string responseData = String.Empty;
        byte[]? responseBytes = null;

        TcpClient? client2 = null;
        NetworkStream? networkStream2 = null;
        Request? request2 = null;

        while (true)
        {
            using (var client = await listener.AcceptTcpClientAsync())//Accept Connection from Clients
            using (var networkStream = client.GetStream())//Create Stream to client, to send and recieve Data
            {
                var requestBytes = new byte[1024]; //Set byte length, TCP cuts Packest at 1024 bytes
                await networkStream.ReadAsync(requestBytes, 0, requestBytes.Length); //Read request from Client (async await => needs to wait for a call)

                //Handle Request with requestBytes
                Request request1 = new Request(requestBytes);


                //handle diffrent routes
                if (request1.Route == "POST /users HTTP/1.1" || request1.Route == "POST /sessions HTTP/1.1")
                {
                    responseData = HandleUserCreationAndLogin(request1.Body, request1.Route);
                }

                else if (request1.Token == String.Empty)
                {
                    responseData = "Error: Unauthorized";
                }

                else if (request1.Route == "POST /packages HTTP/1.1")
                {
                    responseData = HandlePackCreation(request1.Body, request1.Token);
                }

                else if (request1.Route == "POST /transactions/packages HTTP/1.1")
                {
                    responseData = BuysPack(request1.Token);
                }

                else if (request1.Route == "GET /cards HTTP/1.1")
                {
                    responseData = GetCards(request1.Token);
                }

                else if (request1.Route == "PUT /deck HTTP/1.1")
                {
                    responseData = assembleDeck(request1.Token, request1.Body);
                }

                else if (request1.Route == "GET /deck HTTP/1.1")
                {
                    responseData = $"Success: Current Deck: {JsonConvert.SerializeObject(dB.GetCards(MapDeckList(getDeck(request1.Token).Split("\n").ToList<string>()), request1.Token)).Replace(":", ".")}";
                }
                else if (request1.Route == "GET /deck?format=plain HTTP/1.1")
                {
                    responseData = $"Success: Current Deck: {getDeck(request1.Token)}";
                }

                else if (request1.Route.StartsWith("GET /users"))
                {
                    if (request1.UsernameInRoute != String.Empty)
                    {
                        responseData = GetUserData(request1.UsernameInRoute, request1.Token);
                    }
                }

                else if (request1.Route.StartsWith("PUT /users"))
                {
                    if (request1.UsernameInRoute != String.Empty)
                    {
                        responseData = UpdateUserData(request1.UsernameInRoute, request1.Token, request1.Body);
                    }
                }

                else if (request1.Route == "GET /stats HTTP/1.1")
                {
                    responseData = $"Success:Your Elo = {dB.GetStats(request1.Token)}";
                }

                else if (request1.Route == "GET /scoreboard HTTP/1.1")
                {
                    responseData = $"Success:Current Scoreboard:{JsonConvert.SerializeObject(dB.GetScoreBoard(request1.Token)).Replace(":", ".")}";
                }

                else if (request1.Route == "GET /tradings HTTP/1.1")
                {
                    responseData = GetTradingList(request1.Token);
                }
                else if (request1.Route == "POST /tradings HTTP/1.1")
                {
                    responseData = $"Success:Posted tradings:{PostTrading(request1.Token, request1.Body)}";
                }
                else if (request1.Route.StartsWith("DELETE /tradings"))
                {
                    responseData = DeleteTrading(request1.Token, request1.UsernameInRoute);
                }
                else if (request1.Route.StartsWith("POST /tradings"))
                {
                    responseData = TradeDeal(request1.Token, request1.UsernameInRoute, request1.Body);
                }

                else if (request1.Route == "POST /battles HTTP/1.1")
                {
                    //Singals client that he is player 1 and waiting
                    responseBytes = Encoding.UTF8.GetBytes(responseKontrukt + "You are Palyer 1, and currently waiting in the Lobby");
                    networkStream.Write(responseBytes, 0, responseBytes.Length);

                    //Waiting fot next player to join
                    Task secondPlayerTask = Task.Run(async () =>
                    {
                        client2 = await listener.AcceptTcpClientAsync();
                        Console.WriteLine("Second player connected");

                        networkStream2 = client2.GetStream();

                        var requestBytes2 = new byte[1024];
                        await networkStream2.ReadAsync(requestBytes, 0, requestBytes.Length);
                        request2 = new Request(requestBytes);
                        //Verify Token and get username

                        //Singals client that he is player 2
                        var responseBytes2 = Encoding.UTF8.GetBytes(responseKontrukt + "You are Palyer 2");
                        networkStream2.Write(responseBytes2, 0, responseBytes2.Length);

                    });

                    await secondPlayerTask;

                    //Loads the decks from each player
                    var p1Deck = dB.GetCards(MapDeckList(getDeck(request1.Token).Split("\n").ToList<string>()), request1.Token);
                    var p2Deck = dB.GetCards(MapDeckList(getDeck(request2.Token).Split("\n").ToList<string>()), request2.Token);

                    //Loads Battle and starts immidietly
                    Battle b = new Battle(dB, new List<Player> { new Player(client, dB.GetUserDataFromSession(request1.Token), p1Deck), new Player(client2, dB.GetUserDataFromSession(request2.Token), p2Deck) });
                    responseData = $"Success:{b.BeginnTheBattle()}";
                }

                //sends the given response from one of the routes
                Response response = new Response(responseData);
                string res = response.ResConstruct + JsonConvert.SerializeObject(response);

                //sends the response to the client
                responseBytes = Encoding.UTF8.GetBytes(res);
                networkStream.Write(responseBytes, 0, responseBytes.Length);

                //also send res to player 2 when battle is done
                if (request1.Route == "POST /battles HTTP/1.1")
                {
                    networkStream2.Write(responseBytes, 0, responseBytes.Length);
                }
            }
        }
    }


    /// <summary>
    /// I Deserialize the Json object into User object and pass it to the Method which handles the Database communication;
    /// </summary>
    /// <param name="body"></param>
    /// <returns>A string message like Format = Status, Message, Data (if needed)</returns>
    static string HandleUserCreationAndLogin(string body, string route)
    {
        User? user = JsonConvert.DeserializeObject<User>(body);

        if (route == "POST /users HTTP/1.1")
            return dB.AddUser(user);
        else
            return dB.CreateSession(user);
    }

    /// <summary>
    /// forwards to Method
    /// </summary>
    /// <param name="body"></param>
    /// <param name="headerToken"></param>
    /// <returns>string for response</returns>
    static string HandlePackCreation(string body, string headerToken)
    {
        return dB.CreatePackandCards(JsonConvert.DeserializeObject<List<Card>>(body), headerToken); ;
    }

    /// <summary>
    /// Saves and updates everything in the database
    /// </summary>
    /// <param name="headerToken"></param>
    /// <returns>Message</returns>
    static string BuysPack(string headerToken)
    {
        return dB.BuyPack(headerToken);
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

    /// <summary>
    /// forwards to Method but also checks if card length is correct
    /// </summary>
    /// <param name="headerToken"></param>
    /// <param name="body"></param>
    /// <returns>res</returns>
    static string assembleDeck(string headerToken, string body)
    {
        //returns card ids that are now the deck
        List<string>? cardIDs = JsonConvert.DeserializeObject<List<string>>(body);
        //check that cardsid should be 4 otherwise error
        if (cardIDs.Count < 4)
        {
            Console.WriteLine("Not enough cards selected");
            return "Error: Not enough cards selected";
        }
        else
        {
            return dB.assembleDeck(headerToken, cardIDs);
        }
    }

    /// <summary>
    /// forwards to Method
    /// </summary>
    /// <param name="headerToken"></param>
    /// <returns>string for res</returns>
    static string getDeck(string headerToken)
    {
        //will return actual cards
        return dB.GetDeck(headerToken);
    }

    /// <summary>
    /// forwards to Method
    /// </summary>
    /// <param name="username"></param>
    /// <param name="headertoken"></param>
    /// <returns>string for res</returns>
    static string GetUserData(string username, string headertoken)
    {
        return dB.GetUserData(username, headertoken);
    }

    /// <summary>
    /// forwards to Method
    /// </summary>
    /// <param name="username"></param>
    /// <param name="headertoken"></param>
    /// <param name="body"></param>
    /// <returns>string for res</returns>
    static string UpdateUserData(string username, string headertoken, string body)
    {
        User? user = JsonConvert.DeserializeObject<User>(body);
        return dB.UpdateUserData(username, headertoken, user.Image, user.Bio);
    }

    /// <summary>
    /// forwards to Method
    /// </summary>
    /// <param name="headertoken"></param>
    /// <returns>string for res</returns>
    static string GetTradingList(string headertoken)
    {
        return dB.GetTradingDeals(headertoken);
    }

    /// <summary>
    /// forwards to Method
    /// </summary>
    /// <param name="headertoken"></param>
    /// <param name="body"></param>
    /// <returns>string for res</returns>
    static string PostTrading(string headertoken, string body)
    {
        var trade = JsonConvert.DeserializeObject<Trading>(body);
        return dB.PostTradingDeals(headertoken, trade);
    }

    /// <summary>
    /// forwards to Method
    /// </summary>
    /// <param name="headertoken"></param>
    /// <param name="traidingId"></param>
    /// <returns>string for res</returns>
    static string DeleteTrading(string headertoken, string traidingId)
    {
        return dB.DeleteTradingDeals(headertoken, traidingId);
    }

    /// <summary>
    /// forwards to Method
    /// </summary>
    /// <param name="headertoken"></param>
    /// <param name="traidingId"></param>
    /// <param name="body"></param>
    /// <returns>string for res</returns>
    static string TradeDeal(string headertoken, string traidingId, string body)
    {
        string? deserializedBody = JsonConvert.DeserializeObject<string>(body);
        return dB.TradeDeal(headertoken, traidingId, deserializedBody);
    }

    /// <summary>
    /// Map a list of cards => so that just the id is left => needed to automaticaly get cards with id
    /// </summary>
    /// <param name="cards"></param>
    /// <returns>list with just cardids (string)</returns>
    static List<string> MapDeckList(List<string> cards)
    {
        return cards.Select(x => x.Split(",")[0]).ToList();
    }
}


