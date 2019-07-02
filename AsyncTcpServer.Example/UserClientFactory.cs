using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncTcpServer.Example
{
    internal class UserClientFactory : ClientFactory<UserClient>
    {
        public override UserClient Create()
        {
            return new UserClient()
            {
                Encrypt = true,
            };
        }
    }
}
