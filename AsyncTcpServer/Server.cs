// <copyright file="Server.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncTcpServer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AsyncClientServer.Messaging.Compression;
    using AsyncClientServer.Messaging.Cryptography;
    using AsyncClientServer.Messaging.Metadata;
    using AsyncClientServer.Server;
    using Serilog;

    /// <summary>
    /// An asynchronous Tcp server which handles compressing, encryption etc.
    /// </summary>
    public class Server : AsyncSocketListener
    {
        private static ILogger logger;

        private readonly ClientStateFactory clientFactory;
        private ConcurrentDictionary<int, ClientState> connectedClients;

        static Server()
        {
            Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("logs\\TcpClient_.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="stateFactory">When a client connects to the server this creates the proper object.</param>
        public Server(ClientStateFactory stateFactory)
        {
            this.clientFactory = stateFactory;

            this.MessageEncryption = new Aes256();
            this.FileCompressor = new GZipCompression();
            this.FolderCompressor = new ZipCompression();

            this.connectedClients = new ConcurrentDictionary<int, ClientState>();

            this.ServerHasStarted += this.Server_ServerHasStarted;

            this.ClientConnected += this.Server_ClientConnected;
            this.ClientDisconnected += this.Server_ClientDisconnected;

            this.MessageSubmitted += this.Server_MessageSubmitted;

            this.FileReceived += this.Server_FileReceived;
            this.MessageReceived += this.Server_MessageReceived;
            this.ProgressFileReceived += this.Server_ProgressFileReceived;
            this.CustomHeaderReceived += this.Server_CustomHeaderReceived;

            this.MessageFailed += this.Server_MessageFailed;
            this.ErrorThrown += this.Server_ErrorThrown;
        }

        /// <summary>
        /// Gets or sets the default logger for the Server.
        /// </summary>
        public static ILogger Logger
        {
            get => logger;
            set => logger = value;
        }

        /// <summary>
        /// Gets the connected clients.
        /// </summary>
        public IDictionary<int, ClientState> ConnectedClients => this.connectedClients.ToDictionary(x => x.Key, x => x.Value);

        private void Server_ServerHasStarted()
        {
            Logger.Information($"Server has started on {this.Ip}:{this.Port}");
        }

        private void Server_ClientConnected(int id, ISocketInfo clientInfo)
        {
            var client = this.clientFactory.Create();
            client.SocketInfo = clientInfo;

            if (this.connectedClients.TryAdd(id, client))
            {
                Logger.Information($"Client connected from {clientInfo.LocalIPv4}");
            }
        }

        private void Server_ClientDisconnected(int id)
        {
            if (this.connectedClients.TryRemove(id, out ClientState deleted))
            {
                Logger.Information($"Client disconnected from {deleted.SocketInfo.LocalIPv4}");
            }
        }

        private void Server_MessageSubmitted(int id, bool close)
        {
            if (this.connectedClients.TryGetValue(id, out ClientState client))
            {
                Logger.Debug($"Message submitted to {client.SocketInfo.LocalIPv4} with close = {close}");
            }
        }

        private void Server_FileReceived(int id, string filepath)
        {
            if (this.connectedClients.TryGetValue(id, out ClientState client))
            {
                Logger.Debug($"File received from {client.SocketInfo.LocalIPv4} to {filepath}");
                client.Controller.HandleFile(filepath);
            }
        }

        private void Server_MessageReceived(int id, string msg)
        {
            if (this.connectedClients.TryGetValue(id, out ClientState client))
            {
                Logger.Debug($"Message received from {client.SocketInfo.LocalIPv4}{Environment.NewLine}{msg}");
                client.Controller.HandleMessage(msg);
            }
        }

        private void Server_ProgressFileReceived(int id, int bytes, int messageSize)
        {
            if (this.connectedClients.TryGetValue(id, out ClientState client))
            {
                Logger.Debug($"File progress received from {client.SocketInfo.LocalIPv4} {bytes}/{messageSize}");

                // ???
            }
        }

        private void Server_CustomHeaderReceived(int id, string msg, string header)
        {
            if (this.connectedClients.TryGetValue(id, out ClientState client))
            {
                Logger.Debug($"Custom header received from {client.SocketInfo.LocalIPv4}{Environment.NewLine}{msg}{Environment.NewLine}Header: {header}");
            }
        }

        private void Server_MessageFailed(int id, byte[] messageData, string exceptionMessage)
        {
            if (this.connectedClients.TryGetValue(id, out ClientState client))
            {
                Logger.Error($"Message failed to send to {client.SocketInfo.LocalIPv4} with length {messageData.Length}{Environment.NewLine}{exceptionMessage}");
            }
        }

        private void Server_ErrorThrown(string exceptionMessage)
        {
            Logger.Error($"Error thrown{Environment.NewLine}{exceptionMessage}");
        }
    }
}
