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

    /// <summary>
    /// Contains basic information about the client.
    /// </summary>
    public abstract class Client
    {
        /// <summary>
        /// Gets or sets a value indicating whether the communication channel need to be encrypted.
        /// </summary>
        public bool Encrypt { get; set; }
    }
}
