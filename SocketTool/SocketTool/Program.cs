using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTool
{
    class Program
    {
        static void Main(string[] args)
        {
            SocketServer_udp socketServer_Udp = new  SocketServer_udp();
            socketServer_Udp.InitServer(2222);
            SocketServer_udp socketServer_Udp1 = new SocketServer_udp();
            socketServer_Udp1.InitServer(2222);
        }
    }
}
