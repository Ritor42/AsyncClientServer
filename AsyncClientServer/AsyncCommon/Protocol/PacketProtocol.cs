// <copyright file="PacketProtocol.cs" company="PlaceholderCompany">
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
    /// Defines the packet protocol with optional compress, max size etc.
    /// </summary>
    public class PacketProtocol
    {
        private readonly int maxMessageSize;
        private readonly byte[] lengthBuffer;

        private byte[] dataBuffer;
        private int bytesReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="PacketProtocol"/> class.
        /// </summary>
        /// <param name="maxMessageSize">Max size which cannot be exceeded by the received message.</param>
        public PacketProtocol(in int maxMessageSize)
        {
            this.lengthBuffer = new byte[sizeof(int)];
            this.maxMessageSize = maxMessageSize;
        }

        /// <summary>
        /// Fires when a complete message arrived.
        /// </summary>
        public event EventHandler<DataCompletedEventArgs> DataCompleted;

        /// <summary>
        /// Wraps message proper to the protocol with optional compress.
        /// </summary>
        /// <param name="msg">Message that will be wrapped.</param>
        /// <param name="compress">Indicates whether the message will be compressed.</param>
        /// <returns>Wrapped message as bytes.</returns>
        public static byte[] WrapMessage(in string msg, in bool compress)
        {
            var message = compress ? Compress.Zip(msg) : Encoding.UTF8.GetBytes(msg);

            byte[] lengthPrefix = BitConverter.GetBytes(message.Length);
            byte[] ret = new byte[lengthPrefix.Length + message.Length];
            lengthPrefix.CopyTo(ret, 0);
            message.CopyTo(ret, lengthPrefix.Length);

            return ret;
        }

        /// <summary>
        /// Processes the received package.
        /// </summary>
        /// <param name="data">Package that will be processed.</param>
        public void DataReceived(in byte[] data)
        {
            int i = 0;
            while (i != data.Length)
            {
                int bytesAvailable = data.Length - i;
                if (this.dataBuffer != null)
                {
                    int bytesRequested = this.dataBuffer.Length - this.bytesReceived;
                    int bytesTransferred = Math.Min(bytesRequested, bytesAvailable);
                    Array.Copy(data, i, this.dataBuffer, this.bytesReceived, bytesTransferred);
                    i += bytesTransferred;

                    this.ReadCompleted(bytesTransferred);
                }
                else
                {
                    int bytesRequested = this.lengthBuffer.Length - this.bytesReceived;
                    int bytesTransferred = Math.Min(bytesRequested, bytesAvailable);
                    Array.Copy(data, i, this.lengthBuffer, this.bytesReceived, bytesTransferred);
                    i += bytesTransferred;

                    this.ReadCompleted(bytesTransferred);
                }
            }
        }

        private void ReadCompleted(in int count)
        {
            this.bytesReceived += count;

            if (this.dataBuffer == null && this.bytesReceived == sizeof(int))
            {
                int length = BitConverter.ToInt32(this.lengthBuffer, 0);

                if (length < 0)
                {
                    throw new System.Net.ProtocolViolationException("Message length is less than zero");
                }
                else if (this.maxMessageSize > 0 && length > this.maxMessageSize)
                {
                    throw new System.Net.ProtocolViolationException("Message length " + length.ToString(System.Globalization.CultureInfo.InvariantCulture) + " is larger than maximum message size " + this.maxMessageSize.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                else if (length == 0)
                {
                    this.bytesReceived = 0;
                }
                else
                {
                    this.dataBuffer = new byte[length];
                    this.bytesReceived = 0;
                }
            }
            else if (this.dataBuffer != null && this.bytesReceived == this.dataBuffer.Length)
            {
                this.DataCompleted?.Invoke(this, new DataCompletedEventArgs() { Data = Compress.Unzip(this.dataBuffer) });

                this.dataBuffer = null;
                this.bytesReceived = 0;
            }
        }
    }
}
