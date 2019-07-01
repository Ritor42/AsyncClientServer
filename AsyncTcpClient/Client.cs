// <copyright file="Client.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncTcpClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AsyncClientServer.Client;
    using Serilog;

    /// <summary>
    /// An asynchronous Tcp client which handles compressing, encryption etc.
    /// </summary>
    public class Client : AsyncSocketClient
    {
        private static ILogger logger;

        private readonly ClientState state;

        static Client()
        {
            Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("logs\\TcpClient_.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="stateFactory">When a client connects to the server this creates the proper object.</param>
        public Client(ClientStateFactory stateFactory)
        {
            this.state = stateFactory.Create();

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
        public ClientState State => this.state;

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
            this.state.Controller.HandleFile(path);
        }

        private void Client_MessageReceived(SocketClient tcpClient, string msg)
        {
            Logger.Debug($"Message received from {tcpClient.Ip}:{tcpClient.Port}{Environment.NewLine}{msg}");
            this.state.Controller.HandleMessage(msg);
        }

        private void Client_ProgressFileReceived(SocketClient tcpClient, int bytesReceived, int messageSize)
        {
            Logger.Debug($"File progress received from {tcpClient.Ip}:{tcpClient.Port}  {bytesReceived}/{messageSize}");
        }

        private void Client_CustomHeaderReceived(SocketClient tcpClient, string msg, string header)
        {
            Logger.Debug($"Custom header received from {tcpClient.Ip}:{tcpClient.Port}{Environment.NewLine}{msg}{Environment.NewLine}Header: {header}");
            this.state.Controller.HandleCustomHeaderReceived(msg, header);
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
