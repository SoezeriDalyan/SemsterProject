using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        const int port = 10001;
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Listening on Address localhost...");
        Console.WriteLine($"Listening on port {port}...");

        while (true)
        {
            using (var client = await listener.AcceptTcpClientAsync())
            using (var networkStream = client.GetStream())//Create Stream, to send and recieve Data
            {
                var requestBytes = new byte[1024]; //Set byte length, TCP cuts Packest at 1024 bytes
                await networkStream.ReadAsync(requestBytes, 0, requestBytes.Length);
                var request = Encoding.UTF8.GetString(requestBytes);

                //heandle user createn or login request
                if (request.StartsWith("GET /users HTTP/1.1"))
                {
                    //GET User, read User from (later) Postgres
                    string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nHello, World!";// for now plain Text, later change to json
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
                }
                else if (request.StartsWith("POST /users HTTP/1.1"))
                {
                    //POST User, save User into (later) Postgres | ado.net 
                    string responseData = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nPosted data received!";// for now plain Text, later change to json
                    byte[] responseBytes = Encoding.UTF8.GetBytes(responseData);
                    Console.WriteLine(request);
                    await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
                }
            }
        }
    }
}