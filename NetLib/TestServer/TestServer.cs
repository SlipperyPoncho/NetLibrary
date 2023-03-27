using NetLib.Server;

class TestServer
{
    static void Main(string[] args)
    {
        Server server = new(11000);
        server.Start();

        string? input = "";
        while (server.Running) {
            input = Console.ReadLine();
            if (input != null) {
                server.SendString_All(input);
            }
        }
    }
}