// <copyright file="ClientFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncTcpServer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Base client factory which can create a default client.
    /// </summary>
    /// <typeparam name="TClient">Client type.</typeparam>
    public abstract class ClientFactory<TClient> : IClientFactory
        where TClient : Client
    {
        /// <summary>
        /// Creates a default client object.
        /// </summary>
        /// <returns>Client.</returns>
        public abstract TClient Create();

        /// <inheritdoc/>
        Client IClientFactory.Create()
        {
            return this.Create();
        }
    }
}
