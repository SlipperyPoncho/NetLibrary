using NetLib.Server;

class TestServer
{
    static void Main(string[] args)
    {
        Server server = new(11000);
        server.Start();
    }
}