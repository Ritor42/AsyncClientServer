// <copyright file="Server.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncClientServer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AsyncClientServer.Protocol;
    using AsyncNet.Core.Events;
    using AsyncNet.Tcp.Connection.Events;
    using AsyncNet.Tcp.Remote;
    using AsyncNet.Tcp.Remote.Events;
    using AsyncNet.Tcp.Server;
    using AsyncNet.Tcp.Server.Events;
    using Serilog;

    /// <summary>
    /// Tcp server with message (de)fragmenting and handling.
    /// </summary>
    public abstract class Server
    {
        private static ILogger logger;

        private readonly bool toCompress;
        private readonly object lockObject;
        private readonly int maxMessageSize;

        private readonly CancellationTokenSource tokenSource;
        private readonly ConcurrentDictionary<IRemoteTcpPeer, PacketProtocol> peers;

        private IAsyncNetTcpServer tcpServer;

        static Server()
        {
            Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs\\TcpServer_.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="compress">Indicates whether the messages will be compressed or not. Must be same on client.</param>
        /// <param name="maxSize">Maximum message size in bytes that can be received.</param>
        public Server(in bool compress = true, in int maxSize = int.MaxValue)
        {
            this.toCompress = compress;
            this.maxMessageSize = maxSize;
            this.lockObject = new object();

            this.tokenSource = new CancellationTokenSource();
            this.peers = new ConcurrentDictionary<IRemoteTcpPeer, PacketProtocol>();

            this.MessageReceived += (s, e) => this.Server_MessageReceived(this, e);
        }

        /// <summary>
        /// Fires when a message is received from the client
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Gets or sets the logger which the server uses to log information.
        /// </summary>
        public static ILogger Logger
        {
            get => logger;
            set => logger = value;
        }

        /// <summary>
        /// Gets the connected clients.
        /// </summary>
        protected IReadOnlyCollection<IRemoteTcpPeer> Peers => this.peers.Keys as IReadOnlyCollection<IRemoteTcpPeer>;

        /// <summary>
        /// Starts the server on the specified port.
        /// </summary>
        /// <param name="port">Port number of the server.</param>
        public void Start(in int port)
        {
            if (this.tcpServer != null)
            {
                this.Stop();
            }

            lock (this.lockObject)
            {
                this.tcpServer = new AsyncNetTcpServer(port);

                this.tcpServer.FrameArrived += (s, e) => this.Server_FrameArrived(this, e);
                this.tcpServer.ServerStarted += (s, e) => this.Server_ServerStarted(this, e);
                this.tcpServer.ServerStopped += (s, e) => this.Server_ServerStopped(this, e);
                this.tcpServer.ConnectionEstablished += (s, e) => this.Server_ConnectionEstablished(this, e);
                this.tcpServer.ConnectionClosed += (s, e) => this.Server_ConnectionClosed(this, e);

                this.tcpServer.ServerExceptionOccured += (s, e) => this.Server_ServerExceptionOccured(this, e);
                this.tcpServer.RemoteTcpPeerExceptionOccured += (s, e) => this.Server_RemoteTcpPeerExceptionOccured(this, e);
                this.tcpServer.UnhandledExceptionOccured += (s, e) => this.Server_UnhandledExceptionOccured(this, e);

                this.tcpServer.StartAsync(this.tokenSource.Token);
            }
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop()
        {
            lock (this.lockObject)
            {
                if (this.tcpServer != null)
                {
                    this.tokenSource.Cancel();
                    this.tcpServer = null;
                }
            }
        }

        /// <summary>
        /// Sends a message to the specified client.
        /// </summary>
        /// <param name="message">Message that will be sent. Cannot be null.</param>
        /// <param name="peer">Peer where the message will be sent. Cannot be null.</param>
        protected void Send(in string message, in IRemoteTcpPeer peer)
        {
            peer.ThrowIfNull(nameof(peer));
            message.ThrowIfNull(nameof(message));

            peer.Post(this.GetData(message));
        }

        /// <summary>
        /// Sends an async message to the specified client.
        /// </summary>
        /// <param name="message">Message that will be sent. Cannot be null.</param>
        /// <param name="peer">Peer where the message will be sent. Cannot be null.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task SendAsync(string message, IRemoteTcpPeer peer)
        {
            peer.ThrowIfNull(nameof(peer));
            message.ThrowIfNull(nameof(message));

            await peer.SendAsync(this.GetData(message));
        }

        /// <summary>
        /// Sends a message to the specified peers.
        /// </summary>
        /// <param name="message">Message that will be sent. Cannot be null.</param>
        /// <param name="peers">Peers where the message will be sent. Cannot be null.</param>
        protected void Multicast(in string message, in ICollection<IRemoteTcpPeer> peers)
        {
            peers.ThrowIfNull(nameof(peers));
            message.ThrowIfNull(nameof(message));

            foreach (var peer in peers)
            {
                this.Send(message, peer);
            }
        }

        /// <summary>
        /// Sends an async message to the specified peers.
        /// </summary>
        /// <param name="message">Message that will be sent. Cannot be null.</param>
        /// <param name="peers">Peers where the message will be sent. Cannot be null.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task MulticastAsync(string message, ICollection<IRemoteTcpPeer> peers)
        {
            peers.ThrowIfNull(nameof(peers));
            message.ThrowIfNull(nameof(message));

            foreach (var peer in peers)
            {
                await this.SendAsync(message, peer);
            }
        }

        /// <summary>
        /// Sends a message to the connected peers.
        /// </summary>
        /// <param name="message">Message that will be sent. Cannot be null.</param>
        protected void Broadcast(in string message)
        {
            message.ThrowIfNull(nameof(message));
            this.tcpServer.ThrowIfNull(nameof(this.tcpServer));

            if (this.Peers.Count > 0)
            {
                this.tcpServer.Broadcast(this.GetData(message), this.Peers);
            }
        }

        /// <summary>
        /// Sends an async message to the connected peers.
        /// </summary>
        /// <param name="message">Message that will be sent. Cannot be null.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task BroadcastAsync(string message)
        {
            message.ThrowIfNull(nameof(message));
            this.tcpServer.ThrowIfNull(nameof(this.tcpServer));

            if (this.Peers.Count > 0)
            {
                await this.tcpServer.BroadcastAsync(this.GetData(message), this.Peers);
            }
        }

        /// <summary>
        /// Invoked when a message is received from client.
        /// </summary>
        /// <param name="sender">This server.</param>
        /// <param name="e">MessageReceivedEventArgs.</param>
        protected virtual void Server_MessageReceived(Server sender, MessageReceivedEventArgs e)
        {
            Logger.Debug($"Received message from {e.Peer?.IPEndPoint} with length {e.Message?.Length}");
        }

        /// <summary>
        /// Invoked when the server is started.
        /// </summary>
        /// <param name="sender">This server.</param>
        /// <param name="e">TcpServerStartedEventArgs.</param>
        protected virtual void Server_ServerStarted(Server sender, TcpServerStartedEventArgs e)
        {
            Logger.Information($"Server started on {e.ServerAddress}:{e.ServerPort}");
        }

        /// <summary>
        /// Invoked when the server is stopped.
        /// </summary>
        /// <param name="sender">This server.</param>
        /// <param name="e">TcpServerStoppedEventArgs.</param>
        protected virtual void Server_ServerStopped(Server sender, TcpServerStoppedEventArgs e)
        {
            Logger.Information("Server stopped");
        }

        /// <summary>
        /// Invoked when the connection has been established with a client.
        /// </summary>
        /// <param name="sender">This server.</param>
        /// <param name="e">ConnectionEstablishedEventArgs.</param>
        protected virtual void Server_ConnectionEstablished(Server sender, ConnectionEstablishedEventArgs e)
        {
            if (!this.peers.ContainsKey(e.RemoteTcpPeer))
            {
                var packetProtocol = new PacketProtocol(this.maxMessageSize);
                packetProtocol.DataCompleted += (s2, ev) => this.MessageReceived?.Invoke(this, new MessageReceivedEventArgs() { Peer = e.RemoteTcpPeer, Message = ev.Data });

                this.peers[e.RemoteTcpPeer] = packetProtocol;
                Logger.Debug($"Peer connection established at {e.RemoteTcpPeer?.IPEndPoint}");
            }
        }

        /// <summary>
        /// Invoked when the connection has been closed with a client.
        /// </summary>
        /// <param name="sender">This server.</param>
        /// <param name="e">ConnectionClosedEventArgs.</param>
        protected virtual void Server_ConnectionClosed(Server sender, ConnectionClosedEventArgs e)
        {
            Logger.Debug($"Peer connection closed at {e.RemoteTcpPeer?.IPEndPoint}");
        }

        /// <summary>
        /// Invoked when a server exception has been occured.
        /// </summary>
        /// <param name="sender">This server.</param>
        /// <param name="e">TcpServerExceptionEventArgs.</param>
        protected virtual void Server_ServerExceptionOccured(Server sender, TcpServerExceptionEventArgs e)
        {
            Logger.Error(e.Exception, "TcpServer exception occured");
        }

        /// <summary>
        /// Invoked when a remote Tcp exception has been occured.
        /// </summary>
        /// <param name="sender">This server.</param>
        /// <param name="e">RemoteTcpPeerExceptionEventArgs.</param>
        protected virtual void Server_RemoteTcpPeerExceptionOccured(Server sender, RemoteTcpPeerExceptionEventArgs e)
        {
            Logger.Error(e.Exception, "Remote peer exception occured");
        }

        /// <summary>
        /// Invoked when an unhandled exception has been occured.
        /// </summary>
        /// <param name="sender">This server.</param>
        /// <param name="e">ExceptionEventArgs.</param>
        protected virtual void Server_UnhandledExceptionOccured(Server sender, ExceptionEventArgs e)
        {
            Logger.Error(e.Exception, "Unhandled exception occured");
        }

        private void Server_FrameArrived(Server sender, TcpFrameArrivedEventArgs e)
        {
            if (this.peers.TryGetValue(e.RemoteTcpPeer, out PacketProtocol packetProtocol))
            {
                packetProtocol.DataReceived(e.FrameData);
            }
        }

        private byte[] GetData(in string message)
        {
            return PacketProtocol.WrapMessage(message, this.toCompress);
        }
    }
}