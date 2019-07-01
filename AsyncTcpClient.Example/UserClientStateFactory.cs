using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncTcpClient.Example
{
    internal class UserClientStateFactory : ClientStateFactory
    {
        public override ClientState Create()
        {
            return new UserClientState()
            {
                Encrypt = true,
                Controller = new UserClientController(),
            };
        }
    }
}
