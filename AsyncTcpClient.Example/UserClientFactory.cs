// <copyright file="UserClientFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncTcpClient.Example
{
    internal class UserClientFactory : ClientFactory<UserClient>
    {
        public override UserClient Create()
        {
            return new UserClient()
            {
                Encrypt = true,
            };
        }
    }
}
