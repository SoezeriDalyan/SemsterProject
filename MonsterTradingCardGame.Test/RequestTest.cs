using System.Text;

namespace MonsterTradingCardGame.Tests
{
    [TestFixture]
    public class RequestTests
    {
        [Test]
        public void Constructor_ParsesPostRequestWithBodyAndToken()
        {
            // Arrange
            var requestBody = "{'key':'value'}";
            var request = $"POST /sessions HTTP/1.1\r\nAuthorization: Bearer Token\r\nContent-Length: {requestBody.Length}\r\n\r\n{requestBody}";
            var requestBytes = Encoding.UTF8.GetBytes(request);

            // Act
            var requestObj = new Request(requestBytes);

            // Assert
            Assert.AreEqual("POST /sessions HTTP/1.1", requestObj.Route);
            Assert.AreEqual("Bearer Token", requestObj.Token);
            Assert.AreEqual(requestBody, requestObj.Body);
            Assert.AreEqual(String.Empty, requestObj.UsernameInRoute);
        }

        [Test]
        public void Constructor_ParsesGetRequestWithUsernameInRoute()
        {
            // Arrange
            var request = "GET /users/username HTTP/1.1\r\nAuthorization: Bearer Token\r\n\r\n";
            var requestBytes = Encoding.UTF8.GetBytes(request);

            // Act
            var requestObj = new Request(requestBytes);

            // Assert
            Assert.AreEqual("GET /users/username HTTP/1.1", requestObj.Route);
            Assert.AreEqual("Bearer Token", requestObj.Token);
            Assert.AreEqual(String.Empty, requestObj.Body);
            Assert.AreEqual("username", requestObj.UsernameInRoute);
        }

        [Test]
        public void Constructor_ParsesGetRequestWithoutUsernameInRoute()
        {
            // Arrange
            var request = "GET /other/route HTTP/1.1\r\nAuthorization: Bearer Token\r\n\r\n";
            var requestBytes = Encoding.UTF8.GetBytes(request);

            // Act
            var requestObj = new Request(requestBytes);

            // Assert
            Assert.AreEqual("GET /other/route HTTP/1.1", requestObj.Route);
            Assert.AreEqual("Bearer Token", requestObj.Token);
            Assert.AreEqual(String.Empty, requestObj.Body);
            Assert.AreEqual(String.Empty, requestObj.UsernameInRoute);
        }

        [Test]
        public void Constructor_ParsesRequestWithoutAuthorizationHeader()
        {
            // Arrange
            var request = "PUT /sessions HTTP/1.1\r\n\r\n";
            var requestBytes = Encoding.UTF8.GetBytes(request);

            // Act
            var requestObj = new Request(requestBytes);

            // Assert
            Assert.AreEqual("PUT /sessions HTTP/1.1", requestObj.Route);
            Assert.AreEqual(String.Empty, requestObj.Token);
            Assert.AreEqual(String.Empty, requestObj.Body);
            Assert.AreEqual(String.Empty, requestObj.UsernameInRoute);
        }
    }
}
