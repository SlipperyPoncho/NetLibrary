using NetLib_NETStandart;
using System.Net;


class TestClient {
    static void Main(string[] args) {
        Client client = new Client(
            new IPEndPoint(IPAddress.Loopback, 11000)
            );
        client.Start();

        string? input = "";
        while (input != "!exit") {
            input = Console.ReadLine();
            if (input != null) {
                client.SendString(input);
            }
        }
        client.SendDisconnect("Doog");
    }
}