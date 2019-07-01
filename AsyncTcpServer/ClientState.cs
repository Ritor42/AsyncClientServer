﻿// <copyright file="ClientState.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncTcpServer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AsyncClientServer.Messaging.Metadata;

    /// <summary>
    /// Contains basic information about the client.
    /// </summary>
    public abstract class ClientState
    {
        /// <summary>
        /// Gets or sets a value indicating whether the communication channel need to be encrypted.
        /// </summary>
        public bool Encrypt { get; set; }

        /// <summary>
        /// Gets or sets the socket info.
        /// </summary>
        public ISocketInfo SocketInfo { get; set; }

        /// <summary>
        /// Gets or sets the controller.
        /// </summary>
        public ClientController Controller { get; set; }
    }
}
