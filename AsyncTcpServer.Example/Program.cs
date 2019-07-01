using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncTcpServer.Example
{
    class Program
    {
        private static Server server;

        static Program()
        {
            Server.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
        }

        static void Main(string[] args)
        {
            var factory = new UserClientStateFactory();

            server = new Server(factory);
            server.StartListening(4040);

            Task.Run(() => BroadCast());

            Console.ReadLine();
        }

        static async Task BroadCast()
        {
            while(true)
            {
                await server.SendMessageToAllClientsAsync("Hello clients!", false);
                await Task.Delay(500);
            }
        }
    }
}
