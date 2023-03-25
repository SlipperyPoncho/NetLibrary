using NetLib;
using System.Net;

class TestClient
{
    static void Main(string[] args)
    {
        Client client = new Client(
            new IPEndPoint(IPAddress.Loopback, 11000)
            );
        client.Start();
        client.SendString("yo what's goin on bro");

        string? input = "";
        while (input != "!exit") {
            input = Console.ReadLine();
            if (input != null) {
                client.SendString(input);
            }
        }
    }
}