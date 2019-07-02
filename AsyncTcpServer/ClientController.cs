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
    /// <typeparam name="TClient">Client type</typeparam>
    public abstract class ClientController<TClient> : IClientController
        where TClient : ClientState
    {
        /// <summary>
        /// Handles a received file from a client.
        /// </summary>
        /// <param name="client">Sender</param>
        /// <param name="filepath">Received file path.</param>
        public abstract void HandleFile(TClient client, string filepath);

        /// <summary>
        /// Handles a received message from a client.
        /// </summary>
        /// <param name="client">Sender</param>
        /// <param name="message">Received message.</param>
        public abstract void HandleMessage(TClient client, string message);

        /// <summary>
        /// Handles a received custom message from a client.
        /// </summary>
        /// <param name="client">Sender</param>
        /// <param name="message">Received message.</param>
        /// <param name="header">Received custom header.</param>
        public abstract void HandleCustomHeaderReceived(TClient client, string message, string header);

        /// <inheritdoc/>
        public void HandleFile(ClientState client, string filepath)
        {
            if(client is TClient tclient)
            {
                this.HandleFile(tclient, filepath);
            }
        }

        /// <inheritdoc/>
        public void HandleMessage(ClientState client, string message)
        {
            if (client is TClient tclient)
            {
                this.HandleMessage(tclient, message);
            }
        }

        /// <inheritdoc/>
        public void HandleCustomHeaderReceived(ClientState client, string message, string header)
        {
            if (client is TClient tclient)
            {
                this.HandleCustomHeaderReceived(tclient, message, header);
            }
        }
    }
}
