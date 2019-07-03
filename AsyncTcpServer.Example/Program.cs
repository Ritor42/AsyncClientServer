// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncTcpServer.Example
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Serilog;

    internal class Program
    {
        private static TcpServer server;

        static Program()
        {
            TcpServer.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
        }

        private static void Main(string[] args)
        {
            var factories = new Dictionary<string, IClientFactory>();
            var controllers = new Dictionary<Type, IClientController>();

            factories.Add("UserClient", new UserClientFactory());
            controllers.Add(typeof(UserClient), new UserClientController());

            server = new TcpServer(factories, controllers);
            server.StartListening(4040);

            Task.Run(() => BroadCast());

            Console.ReadLine();
        }

        private static async Task BroadCast()
        {
            while (true)
            {
                await server.SendMessageToAllClientsAsync("Hello clients!", false);
                await Task.Delay(500);
            }
        }
    }
}
