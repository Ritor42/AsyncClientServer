// <copyright file="UserClientFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncTcpServer.Example
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

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
