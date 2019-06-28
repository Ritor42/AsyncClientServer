// <copyright file="Client.cs" company="PlaceholderCompany">
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
    using AsyncNet.Tcp.Client;
    using AsyncNet.Tcp.Client.Events;
    using AsyncNet.Tcp.Connection.Events;
    using AsyncNet.Tcp.Remote;
    using AsyncNet.Tcp.Remote.Events;
    using Serilog;

    /// <summary>
    /// Tcp client with message (de)fragmenting and handling.
    /// </summary>
    public abstract class Client
    {
        private static ILogger logger;

        private readonly bool toCompress;
        private readonly bool alwaysReconnect;
        private readonly object lockObject;

        private readonly PacketProtocol packetProtocol;
        private readonly ConcurrentQueue<string> messageQueue;

        private Exception lastException;

        private IRemoteTcpPeer remotePeer;
        private IAsyncNetTcpClient tcpClient;
        private CancellationTokenSource tokenSource;

        static Client()
        {
            Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs\\TcpClient_.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="alwaysReconnect">Reconnect to the server automatically when the client is stopped. </param>
        /// <param name="compress">Indicates whether the messages will be compressed or not. Must be same on server.</param>
        /// <param name="maxSize">Maximum message size in bytes that can be received.</param>
        public Client(in bool alwaysReconnect, in bool compress = true, in int maxSize = int.MaxValue)
        {
            this.toCompress = compress;
            this.lockObject = new object();
            this.alwaysReconnect = alwaysReconnect;

            this.messageQueue = new ConcurrentQueue<string>();
            this.packetProtocol = new PacketProtocol(maxSize);
            this.packetProtocol.DataCompleted += (s, e) => this.MessageReceived?.Invoke(this, new MessageReceivedEventArgs() { Message = e.Data });

            this.MessageReceived += (s, e) => this.Client_MessageReceived(this, e);
            var task = this.ProcessQueue();
        }

        /// <summary>
        /// Fires when a message is received from the server
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Gets or sets the logger which the client uses to log information.
        /// </summary>
        public static ILogger Logger
        {
            get => logger;
            set => logger = value;
        }

        /// <summary>
        /// Connects to the server.
        /// </summary>
        /// <param name="ipAddress">Ip address of the server.</param>
        /// <param name="port">Port number of the server.</param>
        public void Connect(in string ipAddress, in int port)
        {
            if (this.tcpClient != null)
            {
                this.Disconnect();
            }

            lock (this.lockObject)
            {
                this.tokenSource = new CancellationTokenSource();

                this.tcpClient = new AsyncNetTcpClient(ipAddress, port);

                this.tcpClient.FrameArrived += (s, e) => this.TcpClient_FrameArrived(this, e);
                this.tcpClient.ClientStarted += (s, e) => this.Client_ClientStarted(this, e);
                this.tcpClient.ClientStopped += (s, e) => this.Client_ClientStopped(this, e);
                this.tcpClient.ConnectionEstablished += (s, e) => this.Client_ConnectionEstablished(this, e);
                this.tcpClient.ConnectionClosed += (s, e) => this.Client_ConnectionClosed(this, e);

                this.tcpClient.ClientExceptionOccured += (s, e) => this.Client_ClientExceptionOccured(this, e);
                this.tcpClient.RemoteTcpPeerExceptionOccured += (s, e) => this.Client_RemoteTcpPeerExceptionOccured(this, e);
                this.tcpClient.UnhandledExceptionOccured += (s, e) => this.Client_UnhandledExceptionOccured(this, e);

                this.tcpClient.StartAsync(this.tokenSource.Token);
            }
        }

        /// <summary>
        /// Reconnects to the server if a connection has been already made.
        /// </summary>
        public void Reconnect()
        {
            lock (this.lockObject)
            {
                if (this.tcpClient != null)
                {
                    this.Disconnect();
                    this.tokenSource = new CancellationTokenSource();
                    this.tcpClient.StartAsync(this.tokenSource.Token);
                }
            }
        }

        /// <summary>
        /// Disconnects from the server if a connection has been already made.
        /// </summary>
        public void Disconnect()
        {
            lock (this.lockObject)
            {
                if (this.tokenSource != null && !this.tokenSource.IsCancellationRequested)
                {
                    this.tokenSource.Cancel();
                }
            }
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">Message that will be sent. Cannot be null.</param>
        /// <returns>Indicating whether the message is sent. If it couldn't then automatically retries later.</returns>
        public bool Send(string message)
        {
            message.ThrowIfNull(nameof(message));

            if (this.remotePeer != null)
            {
                return this.remotePeer.Post(this.GetData(message));
            }
            else
            {
                this.messageQueue.Enqueue(message);
            }

            return false;
        }

        /// <summary>
        /// Sends an async message to the server.
        /// </summary>
        /// <param name="message">Message that will be sent. Cannot be null.</param>
        /// <returns>Indicating whether the message is sent. If it couldn't then automatically retries later.</returns>
        public async Task<bool> SendAsync(string message)
        {
            message.ThrowIfNull(nameof(message));

            if (this.remotePeer != null)
            {
                return await this.remotePeer.SendAsync(this.GetData(message));
            }
            else
            {
                this.messageQueue.Enqueue(message);
            }

            return false;
        }

        /// <summary>
        /// Invoked when a message is received from the server.
        /// </summary>
        /// <param name="sender">This client.</param>
        /// <param name="e">MessageReceivedEventArgs.</param>
        protected virtual void Client_MessageReceived(Client sender, MessageReceivedEventArgs e)
        {
            Logger.Debug($"Received message from {e.Peer?.IPEndPoint} with length {e.Message?.Length}");
        }

        /// <summary>
        /// Invoked when the client is started.
        /// </summary>
        /// <param name="sender">This client.</param>
        /// <param name="e">TcpClientStartedEventArgs.</param>
        protected virtual void Client_ClientStarted(Client sender, TcpClientStartedEventArgs e)
        {
            Logger.Information($"Client started");
        }

        /// <summary>
        /// Invoked when the client is stopped.
        /// </summary>
        /// <param name="sender">This client.</param>
        /// <param name="e">TcpClientStoppedEventArgs.</param>
        protected virtual void Client_ClientStopped(Client sender, TcpClientStoppedEventArgs e)
        {
            Logger.Information("Client stopped");

            if (this.alwaysReconnect)
            {
                this.Reconnect();
            }
        }

        /// <summary>
        /// Invoked when the connection has been established with the server.
        /// </summary>
        /// <param name="sender">This client.</param>
        /// <param name="e">ConnectionEstablishedEventArgs.</param>
        protected virtual void Client_ConnectionEstablished(Client sender, ConnectionEstablishedEventArgs e)
        {
            this.remotePeer = e.RemoteTcpPeer;
            Logger.Information($"Server connection established at {e.RemoteTcpPeer?.IPEndPoint}");
        }

        /// <summary>
        /// Invoked when the connection has been closed with the server.
        /// </summary>
        /// <param name="sender">This client.</param>
        /// <param name="e">ConnectionClosedEventArgs.</param>
        protected virtual void Client_ConnectionClosed(Client sender, ConnectionClosedEventArgs e)
        {
            Logger.Information($"Server connection closed at {e.RemoteTcpPeer?.IPEndPoint}");
        }

        /// <summary>
        /// Invoked when a client exception has been occured.
        /// </summary>
        /// <param name="sender">This client.</param>
        /// <param name="e">TcpClientExceptionEventArgs.</param>
        protected virtual void Client_ClientExceptionOccured(Client sender, TcpClientExceptionEventArgs e)
        {
            if (e.Exception.Message != this.lastException?.Message)
            {
                this.lastException = e.Exception;
                Logger.Error(e.Exception, $"TcpClient exception occured at {DateTime.Now}");
            }
            else
            {
                Logger.Error($"TcpClient exception occured again at {DateTime.Now}");
            }
        }

        /// <summary>
        /// Invoked when a remote Tcp exception has been occured.
        /// </summary>
        /// <param name="sender">This client.</param>
        /// <param name="e">RemoteTcpPeerExceptionEventArgs.</param>
        protected virtual void Client_RemoteTcpPeerExceptionOccured(Client sender, RemoteTcpPeerExceptionEventArgs e)
        {
            if (e.Exception.Message != this.lastException?.Message)
            {
                this.lastException = e.Exception;
                Logger.Error(e.Exception, "Remote peer exception occured");
            }
            else
            {
                Logger.Error($"Remote peer exception occured again at {DateTime.Now}");
            }
        }

        /// <summary>
        /// Invoked when an unhandled exception has been occured.
        /// </summary>
        /// <param name="sender">This client.</param>
        /// <param name="e">ExceptionEventArgs.</param>
        protected virtual void Client_UnhandledExceptionOccured(Client sender, ExceptionEventArgs e)
        {
            if (e.Exception.Message != this.lastException?.Message)
            {
                this.lastException = e.Exception;
                Logger.Error(e.Exception, $"Unhandled exception occured at {DateTime.Now}");
            }
            else
            {
                Logger.Error($"Unhandled exception occured again at {DateTime.Now}");
            }
        }

        private void TcpClient_FrameArrived(Client sender, TcpFrameArrivedEventArgs e)
        {
            this.packetProtocol.DataReceived(e.FrameData);
        }

        private async Task ProcessQueue()
        {
            while (true)
            {
                if (this.messageQueue.Count > 0)
                {
                    if (this.remotePeer != null && this.messageQueue.TryDequeue(out string message))
                    {
                        await this.SendAsync(message);
                    }
                }

                await Task.Delay(100);
            }
        }

        private byte[] GetData(in string message)
        {
            return PacketProtocol.WrapMessage(message, this.toCompress);
        }
    }
}