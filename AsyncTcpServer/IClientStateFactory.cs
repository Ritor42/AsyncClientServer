namespace AsyncTcpServer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines basic logic for client state factories.
    /// </summary>
    public interface IClientStateFactory
    {
        /// <summary>
        /// Creates a default client object.
        /// </summary>
        /// <returns>Client.</returns>
        ClientState Create();
    }
}
