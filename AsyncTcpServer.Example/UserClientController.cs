using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncTcpServer;

namespace AsyncTcpServer.Example
{
    internal class UserClientController : ClientController
    {
        public override void HandleFile(string filepath)
        {
            Console.WriteLine($"New file received: {filepath}");
        }

        public override void HandleMessage(string message)
        {
            Console.WriteLine($"New message received: {message}");
        }

        public override void HandleCustomHeaderReceived(string message, string header)
        {
            Console.WriteLine($"New custom message received: {message} with {header} header.");
        }
    }
}
