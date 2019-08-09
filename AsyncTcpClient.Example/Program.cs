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
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();
        }

        private static void Main(string[] args)
        {
            var factory = new UserClientFactory();
            var controller = new UserClientController();

            client = new TcpClient(factory, controller);
            client.StartClient("127.0.0.1", 4040);
            client.SendMessage("UserClient", false, false);
            Send();

            Console.ReadLine();
        }

        private static void Send()
        {
            Parallel.For(0, 10, i =>
            {
                client.SendMessage("{\"$id\":\"1\",\"$type\":\"Common.Message.Request, Common\",\"ID\":17,\"Item\":\"Data.ShotTaskType, Data, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null\",\"Message\":null,\"Method\":1}", false, false);
                client.SendMessage("{\"$id\":\"1\",\"$type\":\"Common.Message.Request, Common\",\"ID\":17,\"Item\":\"Data.AssetTaskType, Data, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null\",\"Message\":null,\"Method\":1}", false, false);
                client.SendMessage("{\"$id\":\"1\",\"$type\":\"Common.Message.Request, Common\",\"ID\":17,\"Item\":\"Data.AssetClass, Data, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null\",\"Message\":null,\"Method\":1}", false, false);
                client.SendMessage("{\"$id\":\"1\",\"$type\":\"Common.Message.Request, Common\",\"ID\":17,\"Item\":\"Data.Status, Data, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null\",\"Message\":null,\"Method\":1}", false, false);
                client.SendMessage("{\"$id\":\"1\",\"$type\":\"Common.Message.Request, Common\",\"ID\":17,\"Item\":\"Data.Folder, Data, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null\",\"Message\":null,\"Method\":1}", false, false);
                client.SendMessage("{\"$id\":\"1\",\"$type\":\"Common.Message.Request, Common\",\"ID\":17,\"Item\":\"Data.Project, Data, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null\",\"Message\":null,\"Method\":1}", false, false);
                client.SendMessage("{\"$id\":\"1\",\"$type\":\"Common.Message.Request, Common\",\"ID\":17,\"Item\":\"Data.Project, Data, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null\",\"Message\":null,\"Method\":1}", false, false);
                client.SendMessage("{\"$id\":\"1\",\"$type\":\"Common.Message.Request, Common\",\"ID\":17,\"Item\":\"Data.Project, Data, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null\",\"Message\":null,\"Method\":1}", false, false);
                client.SendMessage("{\"$id\":\"1\",\"$type\":\"Common.Message.Request, Common\",\"ID\":17,\"Item\":\"Data.Project, Data, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null\",\"Message\":null,\"Method\":1}", false, false);
                client.SendMessage("{\"$id\":\"1\",\"$type\":\"Common.Message.Request, Common\",\"ID\":17,\"Item\":\"Data.ShotTaskType, Data, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null\",\"Message\":null,\"Method\":1}", false, false);
            });
        }
    }
}
