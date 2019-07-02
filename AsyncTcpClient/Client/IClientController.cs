// <copyright file="IClientController.cs" company="PlaceholderCompany">
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
    /// Defines basic logic for client controllers.
    /// </summary>
    public interface IClientController
    {
        /// <summary>
        /// Gets or sets the Tcp client for communication with the server.
        /// </summary>
        TcpClient TcpClient { get; set; }

        /// <summary>
        /// Handles a received file from a client.
        /// </summary>
        /// <param name="client">Client.</param>
        /// <param name="filepath">Received file path.</param>
        void HandleFile(in Client client, in string filepath);

        /// <summary>
        /// Handles a received message from a client.
        /// </summary>
        /// <param name="client">Client.</param>
        /// <param name="message">Received message.</param>
        void HandleMessage(in Client client, in string message);

        /// <summary>
        /// Handles a received custom message from a client.
        /// </summary>
        /// <param name="client">Client.</param>
        /// <param name="message">Received message.</param>
        /// <param name="header">Received custom header.</param>
        void HandleCustomHeaderReceived(in Client client, in string message, in string header);
    }
}
