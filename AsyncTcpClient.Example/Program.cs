using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncTcpClient.Example
{
    class Program
    {
        private static Client client;

        static Program()
        {
            Client.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
        }

        static void Main(string[] args)
        {
            var factory = new UserClientStateFactory();

            client = new Client(factory);
            client.StartClient("127.0.0.1", 4040);

            Task.Run(() => Send());

            Console.ReadLine();
        }

        static async Task Send()
        {
            while (true)
            {
                await client.SendMessageAsync("Hello server!", false);
                await Task.Delay(500);
            }
        }
    }
}
