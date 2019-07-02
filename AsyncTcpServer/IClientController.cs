namespace AsyncTcpServer
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
        /// Handles a received file from a client.
        /// </summary>
        /// <param name="client">Sender</param>
        /// <param name="filepath">Received file path.</param>
        void HandleFile(ClientState client, string filepath);

        /// <summary>
        /// Handles a received message from a client.
        /// </summary>
        /// <param name="client">Sender</param>
        /// <param name="message">Received message.</param>
        void HandleMessage(ClientState client, string message);

        /// <summary>
        /// Handles a received custom message from a client.
        /// </summary>
        /// <param name="client">Sender</param>
        /// <param name="message">Received message.</param>
        /// <param name="header">Received custom header.</param>
        void HandleCustomHeaderReceived(ClientState client, string message, string header);
    }
}
