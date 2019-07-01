// <copyright file="DataCompletedEventArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncClientServer.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Contains a fully received message.
    /// </summary>
    public class DataCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the received message.
        /// </summary>
        public string Data { get; set; }
    }
}
