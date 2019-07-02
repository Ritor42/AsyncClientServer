namespace AsyncTcpServer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Base client factory which can create a default client.
    /// </summary>
    /// <typeparam name="TClient">Client type</typeparam>
    public abstract class ClientStateFactory<TClient> : IClientStateFactory
        where TClient : ClientState
    {
        /// <summary>
        /// Creates a default client object.
        /// </summary>
        /// <returns>Client</returns>
        public abstract TClient Create();


        /// <inheritdoc/>
        ClientState IClientStateFactory.Create()
        {
            return this.Create();
        }
    }
}
