// <copyright file="UserClientController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncTcpClient.Example
{
    using System;

    internal class UserClientController : ClientController<UserClient>
    {
        public override void HandleFile(in UserClient client, in string filepath)
        {
            Console.WriteLine($"New file received: {filepath}");
        }

        public override void HandleMessage(in UserClient client, in string message)
        {
            Console.WriteLine($"New message received: {message}");
        }

        public override void HandleCustomHeaderReceived(in UserClient client, in string message, in string header)
        {
            Console.WriteLine($"New custom message received: {message} with {header} header.");
        }
    }
}
