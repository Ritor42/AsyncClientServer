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
        private static TcpClient client;

        static Program()
        {
            TcpClient.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
        }

        static void Main(string[] args)
        {
            var factory = new UserClientFactory();
            var controller = new UserClientController();

            client = new TcpClient(factory, controller);
            client.StartClient("127.0.0.1", 4040);
            client.SendMessage("UserClient", false);
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
