// <copyright file="UserClientController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncTcpClient.Example
{
    using System;

    internal class UserClientController : ClientController<UserClient>
    {
        public override void HandleFile(UserClient client, string filepath)
        {
            Console.WriteLine($"New file received: {filepath}");
        }

        public override void HandleMessage(UserClient client, string message)
        {
            Console.WriteLine($"New message received: {message?.Length}");
        }

        public override void HandleCustomHeaderReceived(UserClient client, string message, string header)
        {
            Console.WriteLine($"New custom message received: {message} with {header} header.");
        }
    }
}
