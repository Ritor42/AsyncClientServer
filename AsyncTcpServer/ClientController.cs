// <copyright file="ClientController.cs" company="PlaceholderCompany">
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
    /// Base controller for client messages.
    /// </summary>
    public abstract class ClientController
    {
        // HOGYAN KÜLDJÜNK VISSZA ADATOT?

        /// <summary>
        /// Handles a received file from a client.
        /// </summary>
        /// <param name="filepath">Received file path.</param>
        public abstract void HandleFile(string filepath);

        /// <summary>
        /// Handles a received message from a client.
        /// </summary>
        /// <param name="message">Received message.</param>
        public abstract void HandleMessage(string message);

        /// <summary>
        /// Handles a received custom message from a client.
        /// </summary>
        /// <param name="message">Received message.</param>
        /// <param name="header">Received custom header.</param>
        public abstract void HandleCustomHeaderReceived(string message, string header);
    }
}
