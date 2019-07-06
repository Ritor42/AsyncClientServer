// <copyright file="ClientController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncTcpServer
{
    /// <summary>
    /// Base controller for client messages.
    /// </summary>
    /// <typeparam name="TClient">Client type.</typeparam>
    public abstract class ClientController<TClient> : IClientController
        where TClient : Client
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientController{TClient}"/> class.
        /// </summary>
        public ClientController()
        {
        }

        /// <inheritdoc/>
        public TcpServer TcpServer { get; set; }

        /// <summary>
        /// Handles a received file from a client.
        /// </summary>
        /// <param name="client">Client.</param>
        /// <param name="filepath">Received file path.</param>
        public abstract void HandleFile(TClient client, string filepath);

        /// <summary>
        /// Handles a received message from a client.
        /// </summary>
        /// <param name="client">Client.</param>
        /// <param name="message">Received message.</param>
        public abstract void HandleMessage(TClient client, string message);

        /// <summary>
        /// Handles a received custom message from a client.
        /// </summary>
        /// <param name="client">Client.</param>
        /// <param name="message">Received message.</param>
        /// <param name="header">Received custom header.</param>
        public abstract void HandleCustomHeaderReceived(TClient client, string message, string header);

        /// <inheritdoc/>
        public void HandleFile(in Client client, in string filepath)
        {
            if (client is TClient tclient)
            {
                this.HandleFile(tclient, filepath);
            }
        }

        /// <inheritdoc/>
        public void HandleMessage(in Client client, in string message)
        {
            if (client is TClient tclient)
            {
                this.HandleMessage(tclient, message);
            }
        }

        /// <inheritdoc/>
        public void HandleCustomHeaderReceived(in Client client, in string message, in string header)
        {
            if (client is TClient tclient)
            {
                this.HandleCustomHeaderReceived(tclient, message, header);
            }
        }
    }
}
