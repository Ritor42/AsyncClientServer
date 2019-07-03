// <copyright file="TcpServer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncTcpServer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using AsyncClientServer.Messaging.Metadata;
    using AsyncClientServer.Server;
    using Serilog;

    /// <summary>
    /// An asynchronous Tcp server which handles compressing, encryption etc.
    /// </summary>
    public class TcpServer : AsyncSocketListener
    {
        private static ILogger logger;

        private IDictionary<string, IClientFactory> clientFactories;
        private IDictionary<Type, IClientController> clientControllers;
        private ConcurrentDictionary<int, Client> connectedClients;

        static TcpServer()
        {
            Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("logs\\TcpClient_.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServer"/> class.
        /// </summary>
        /// <param name="factories">Client factories which will create the proper object.</param>
        /// <param name="controllers">Controllers that will handled the proper objects.</param>
        public TcpServer(
            in IDictionary<string, IClientFactory> factories,
            in IDictionary<Type, IClientController> controllers)
        {
            this.connectedClients = new ConcurrentDictionary<int, Client>();
            this.clientFactories = factories.ToDictionary(x => x.Key, x => x.Value) ?? throw new ArgumentNullException(nameof(factories));
            this.clientControllers = controllers.ToDictionary(x => x.Key, x => x.Value) ?? throw new ArgumentNullException(nameof(controllers));
            this.clientControllers.Values.ToList().ForEach(x => x.TcpServer = this);

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
        public IDictionary<int, Client> ConnectedClients => this.connectedClients.ToDictionary(x => x.Key, x => x.Value);

        private void Server_ServerHasStarted()
        {
            Logger.Information($"Server has started on {this.Ip}:{this.Port}");
        }

        private void Server_ClientConnected(int id, ISocketInfo clientInfo)
        {
            Logger.Information($"Client connected from {clientInfo.LocalIPv4}");
        }

        private void Server_ClientDisconnected(int id)
        {
            if (this.connectedClients.TryRemove(id, out Client deleted))
            {
                Logger.Information($"Client disconnected from {deleted.SocketInfo.LocalIPv4}");
            }
        }

        private void Server_MessageSubmitted(int id, bool close)
        {
            if (this.connectedClients.TryGetValue(id, out Client client))
            {
                Logger.Debug($"Message submitted to {client.SocketInfo.LocalIPv4} with close = {close}");
            }
        }

        private void Server_FileReceived(int id, string filepath)
        {
            if (this.connectedClients.TryGetValue(id, out Client client))
            {
                Logger.Debug($"File received from {client.SocketInfo.LocalIPv4} to {filepath}");
                this.clientControllers[client.GetType()].HandleFile(client, filepath);
            }
        }

        private void Server_MessageReceived(int id, string msg)
        {
            if (this.connectedClients.TryGetValue(id, out Client client))
            {
                Logger.Debug($"Message received from {client.SocketInfo.LocalIPv4}{Environment.NewLine}{msg}");
                this.clientControllers[client.GetType()].HandleMessage(client, msg);
            }
            else
            {
                Logger.Debug($"Message received to determine which type of client it is.{Environment.NewLine}{msg}");
                if (this.clientFactories.ContainsKey(msg))
                {
                    var factory = this.clientFactories[msg];

                    var clientState = factory.Create();
                    clientState.ID = id;
                    clientState.SocketInfo = this.GetConnectedClients()[id];
                    this.connectedClients[id] = clientState;

                    Logger.Debug($"Successful definition: {factory.GetType()}->{clientState.GetType()}");
                }
                else
                {
                    Logger.Debug($"Unsuccessful definition.");
                }
            }
        }

        private void Server_ProgressFileReceived(int id, int bytes, int messageSize)
        {
            if (this.connectedClients.TryGetValue(id, out Client client))
            {
                Logger.Debug($"File progress received from {client.SocketInfo.LocalIPv4} {bytes}/{messageSize}");

                // ???
            }
        }

        private void Server_CustomHeaderReceived(int id, string msg, string header)
        {
            if (this.connectedClients.TryGetValue(id, out Client client))
            {
                Logger.Debug($"Custom header received from {client.SocketInfo.LocalIPv4}{Environment.NewLine}{msg}{Environment.NewLine}Header: {header}");
                this.clientControllers[client.GetType()].HandleCustomHeaderReceived(client, msg, header);
            }
        }

        private void Server_MessageFailed(int id, byte[] messageData, string exceptionMessage)
        {
            if (this.connectedClients.TryGetValue(id, out Client client))
            {
                Logger.Error($"Message failed to send to {client.SocketInfo.LocalIPv4} with length {messageData.Length}{Environment.NewLine}{exceptionMessage}");
            }
        }

        private void Server_ErrorThrown(string exceptionMessage)
        {
            Logger.Error($"Error thrown:{Environment.NewLine}{exceptionMessage}");
        }
    }
}
