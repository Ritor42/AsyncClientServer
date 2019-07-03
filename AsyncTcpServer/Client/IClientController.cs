// <copyright file="IClientController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncTcpServer
{
    /// <summary>
    /// Defines basic logic for client controllers.
    /// </summary>
    public interface IClientController
    {
        /// <summary>
        /// Gets or sets the Tcp server for communication with the client.
        /// </summary>
        TcpServer TcpServer { get; set; }

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
