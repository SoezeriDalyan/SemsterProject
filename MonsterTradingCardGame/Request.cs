using System.Text;

namespace MonsterTradingCardGame
{
    public class Request
    {
        //This class deals with every request that is received.
        public string RequestFromUser { get; private set; }
        public string Route { get; private set; }
        public string Token { get; private set; }
        public string Body { get; private set; }
        public string UsernameInRoute { get; private set; }

        public Request(byte[] requestFromUser)
        {
            RequestFromUser = Encoding.UTF8.GetString(requestFromUser);
            Route = GetRoute().Replace("\r", "");
            Body = GetRequestBody();
            Token = GetHeaderToken();
            UsernameInRoute = GetUsernameFromRequestUrl();
        }

        /// <summary>
        /// Gets the Route in the request
        /// </summary>
        /// <returns>The Route like POST /sessions HTTP/1.1</returns>
        private string GetRoute()
        {
            string[] requestLines = RequestFromUser.Split('\n');

            return requestLines[0];
        }

        /// <summary>
        /// Extracts the Token from the Header
        /// </summary>
        /// <returns>Example: Authorization: Bearer Token; Output = Bearer Token</returns>
        private string GetHeaderToken()
        {
            string[] requestLines = RequestFromUser.Split('\n');
            foreach (string line in requestLines)
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
        /// <returns>The Json Data as String</returns>
        private string GetRequestBody()
        {
            string[] requestLines = RequestFromUser.Split('\n');

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
        /// <returns>Username</returns>
        private string GetUsernameFromRequestUrl()
        {
            // Assuming the format is "/users/{username}"
            string[] segments = RequestFromUser.Split('/');
            if (segments.Length >= 3 && segments[1] == "users" || segments[1] == "tradings")
            {
                return segments[2].Split(" ")[0];
            }
            else
            {
                return String.Empty;
            }
        }
    }
}
