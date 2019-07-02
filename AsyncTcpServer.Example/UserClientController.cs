using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncTcpServer;

namespace AsyncTcpServer.Example
{
    internal class UserClientController : ClientController<UserClient>
    {
        public override void HandleFile(in UserClient client, in string filepath)
        {
            Console.WriteLine($"New file received: {filepath}");
        }

        public override void HandleMessage(in UserClient client, in string message)
        {
            Console.WriteLine($"New message received: {message}");
        }

        public override void HandleCustomHeaderReceived(in UserClient client, in string message, in string header)
        {
            Console.WriteLine($"New custom message received: {message} with {header} header.");
        }
    }
}
