// <copyright file="IClientFactory.cs" company="PlaceholderCompany">
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
    /// Defines basic logic for client state factories.
    /// </summary>
    public interface IClientFactory
    {
        /// <summary>
        /// Creates a default client object.
        /// </summary>
        /// <returns>Client.</returns>
        Client Create();
    }
}
