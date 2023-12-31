﻿using System.Text.Json.Serialization;

namespace MonsterTradingCardGame
{
    internal class Response
    {
        [JsonPropertyName("Status")]
        public string Status { get; private set; }
        [JsonPropertyName("Message")]
        public string Message { get; private set; }
        [JsonPropertyName("Data")]
        public dynamic Data { get; private set; }
        [JsonPropertyName("StatusCode")]
        public int StatusCode { get; private set; }
        [Newtonsoft.Json.JsonIgnore]
        public string ResConstruct { get; private set; }
        [Newtonsoft.Json.JsonIgnore]
        public string MappedStatus { get; private set; }

        public Response(string input)
        {
            string[] strings = input.Split(":");

            Message = strings[1];

            Status = strings[0];
            if (Status == "Success")
            {
                StatusCode = 200;
                MappedStatus = "OK";
            }
            else if (Status == "Error" && !Message.Contains("Unauthorized"))
            {
                StatusCode = 400;
                MappedStatus = "Bad Request";
            }
            else
            {
                StatusCode = 401;
                MappedStatus = "Unauthorized";
            }


            if (strings.Length == 3 && strings[2] != null)
            {
                Data = strings[2];

                if (strings[2].Contains("\n"))
                {
                    Data = strings[2].Split("\n").ToArray<string>();
                }
            }

            ResConstruct = $"HTTP/1.1 {StatusCode} {MappedStatus}\r\nContent-Type: text/application/json\r\n\r\n";
        }
    }
}
