using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTool
{
    class SocketClinet_udp
    {
        const int port = 500;
        private  Socket socket;
         IPEndPoint ep;

         public SocketClinet_udp(string ServerAddress, int port)
        {
            // Debug.Log("");
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPAddress serverIP = IPAddress.Parse(ServerAddress);
                ep = new IPEndPoint(serverIP, port);
            }
            catch (Exception ex)
            {
            }
        }
        public  void Send(byte [] bytes)
        {
            try
            {
                socket.SendTo(bytes, ep);
               
            }
            catch (Exception ex)
            {
               
            }
        }

    }
}
