// <copyright file="TcpClient.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncTcpClient
{
    using System;
    using AsyncClientServer.Client;
    using Serilog;

    /// <summary>
    /// An asynchronous Tcp client which handles compressing, encryption etc.
    /// </summary>
    public class TcpClient : AsyncSocketClient
    {
        private static ILogger logger;
        private readonly Client client;
        private readonly IClientController controller;

        static TcpClient()
        {
            Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("logs\\TcpClient_.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        /// <param name="factory">Client factories which will create the proper object.</param>
        /// <param name="controller">Controllers that will handled the proper objects.</param>
        public TcpClient(
            IClientFactory factory,
            IClientController controller)
        {
            this.client = factory.Create() ?? throw new ArgumentNullException(nameof(factory));
            this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
            this.controller.TcpClient = this;

            this.Connected += this.Client_Connected;
            this.Disconnected += this.Client_Disconnected;

            this.MessageSubmitted += this.Client_MessageSubmitted;

            this.FileReceived += this.Client_FileReceived;
            this.MessageReceived += this.Client_MessageReceived;
            this.ProgressFileReceived += this.Client_ProgressFileReceived;
            this.CustomHeaderReceived += this.Client_CustomHeaderReceived;

            this.MessageFailed += this.Client_MessageFailed;
            this.ErrorThrown += this.Client_ErrorThrown;
        }

        /// <summary>
        /// Gets or sets the default logger for the Client.
        /// </summary>
        public static ILogger Logger
        {
            get => logger;
            set => logger = value;
        }

        /// <summary>
        /// Gets the client's state.
        /// </summary>
        public Client Client => this.client;

        private void Client_Connected(SocketClient tcpClient)
        {
            Logger.Information($"Connected to {tcpClient.Ip}:{tcpClient.Port}");
        }

        private void Client_Disconnected(SocketClient tcpClient, string ipServer, int port)
        {
            Logger.Information($"Disconnected from {tcpClient.Ip}:{tcpClient.Port}");
        }

        private void Client_MessageSubmitted(SocketClient tcpClient, bool close)
        {
            Logger.Debug($"Message submitted to {tcpClient.Ip}:{tcpClient.Port} with close = {close}");
        }

        private void Client_FileReceived(SocketClient tcpClient, string path)
        {
            Logger.Debug($"File received from {tcpClient.Ip}:{tcpClient.Port} to {path}");
            this.controller.HandleFile(this.client, path);
        }

        private void Client_MessageReceived(SocketClient tcpClient, string msg)
        {
            Logger.Debug($"Message received from {tcpClient.Ip}:{tcpClient.Port}{Environment.NewLine}{msg}");
            this.controller.HandleMessage(this.client, msg);
        }

        private void Client_ProgressFileReceived(SocketClient tcpClient, int bytesReceived, int messageSize)
        {
            Logger.Debug($"File progress received from {tcpClient.Ip}:{tcpClient.Port}  {bytesReceived}/{messageSize}");

            // ?
        }

        private void Client_CustomHeaderReceived(SocketClient tcpClient, string msg, string header)
        {
            Logger.Debug($"Custom header received from {tcpClient.Ip}:{tcpClient.Port}{Environment.NewLine}{msg}{Environment.NewLine}Header: {header}");
            this.controller.HandleCustomHeaderReceived(this.client, msg, header);
        }

        private void Client_MessageFailed(SocketClient tcpClient, byte[] messageData, string exceptionMessage)
        {
            Logger.Error($"Message failed to send to {tcpClient.Ip}:{tcpClient.Port} with length {messageData.Length}{Environment.NewLine}{exceptionMessage}");
        }

        private void Client_ErrorThrown(SocketClient tcpClient, string exceptionMessage)
        {
            Logger.Error($"Error thrown at {tcpClient.Ip}:{tcpClient.Port}{Environment.NewLine}{exceptionMessage}");
        }
    }
}
