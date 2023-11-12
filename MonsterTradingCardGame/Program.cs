using MonsterTradingCardGame;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static Pack pack = new Pack();
    static DB dB = new DB();
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
                    if (HandleUserLogin(body))
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
                        responseData = $"{responseKontrukt}Pack Creation Failed";
                    }
                    else if (HandlePackCreation(body, headerToken))
                    {
                        responseData = $"{responseKontrukt}Pack Created Succsesfully";
                    }
                }

                /*
                 echo 7) acquire newly created packages altenhof
                 curl -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" 
                --header "Authorization: Bearer altenhof-mtcgToken" -d ""
                */





                Console.WriteLine(responseData);
                responseBytes = Encoding.UTF8.GetBytes(responseData);
                await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
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
        return dB.AddUser(user); ;
    }

    /// <summary>
    /// I Deserialize the Json object into User object and pass it to the Method which handles the Database communication;
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    static bool HandleUserLogin(string body)
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

    static void BuyPacks(string Token)
    {
        /// <summary>
        /// Checking if Session Exists => maybe own method for Check => TOdo
        /// </summary>
        /// <param name="card"></param>
        /// <param name="token"></param>
    }
}
