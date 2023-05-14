using NetLib_NETStandart.Server;

class TestServer {
    static void Main(string[] args) {
        Server server = new(11000);
        server.Start();

        string? input = "";
        while (server.Running) {
            input = Console.ReadLine();
            if (input != null && input != "h") {
                server.SendString_All(input);
            }
            if (input == "h") {
                server.SendHeartbeat_All(DateTime.Now);
            }
        }
    }
}