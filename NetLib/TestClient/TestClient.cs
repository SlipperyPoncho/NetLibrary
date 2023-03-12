using NetLib;
using System.Net;

class TestClient
{
    static void Main(string[] args)
    {
        Client client = new();
        client.Start(11000);
        client.Send("127.0.0.1","hello man");
    }
}