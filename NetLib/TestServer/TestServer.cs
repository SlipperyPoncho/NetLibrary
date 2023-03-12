using NetLib.Server;

class TestServer
{
    static void Main(string[] args)
    {
        Server server = new();
        server.Start(11000);
    }
}