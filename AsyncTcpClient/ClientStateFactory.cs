// <copyright file="ClientStateFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncTcpClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Base client factory which can create a default client.
    /// </summary>
    public abstract class ClientStateFactory
    {
        /// <summary>
        /// Creates a default client.
        /// </summary>
        /// <returns>Client.</returns>
        public abstract ClientState Create();
    }
}
