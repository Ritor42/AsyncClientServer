// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncTcpClient.Example
{
    using System;
    using System.Threading.Tasks;
    using Serilog;

    public class Program
    {
        private static TcpClient client;

        static Program()
        {
            TcpClient.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
        }

        private static void Main(string[] args)
        {
            var factory = new UserClientFactory();
            var controller = new UserClientController();

            client = new TcpClient(factory, controller);
            client.StartClient("127.0.0.1", 4040);
            client.SendMessage("UserClient", false);
            Task.Run(() => Send());

            Console.ReadLine();
        }

        private static async Task Send()
        {
            while (true)
            {
                await client.SendMessageAsync("Hello server!", false);
                await Task.Delay(500);
            }
        }
    }
}
