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
    /// <summary>
    /// 参考SocketServer.cs
    /// 去掉死循环接收消息并打印
    /// 去掉单例模式
    /// 去掉监听客户端连接请求
    /// 同步还是异步
    /// </summary>
    class SocketServer_udp
    {
        //成员&属性
         byte[] buffer = new byte[1024 * 1024];
         Socket serverSocket;
         SocketServer_udp socketServer_udp = null;
         byte[] DataReceived;
         private  Queue<byte[]> dataReceivedQueue = new Queue<byte[]>();
         byte[] tmpbytes;
         readonly Mutex mutex = new Mutex();
        public delegate void ProcessMethod(byte[] data, int len);
        public ProcessMethod processMethod;
        public int port { get; set; }
        /// <summary>
        /// 接收数据
        /// </summary>
         Thread receiveThread;

       public SocketServer_udp()
        {
            port = 2222;
        }
        public bool  InitServer(int port)
        {
            ProtocolType protocolType = ProtocolType.Udp;
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            }
            catch(System.Net.Sockets.SocketException e)
            {
                return false;
            }
            Console.WriteLine("{0} Listining......", serverSocket.LocalEndPoint.ToString());//Console可替换成Debug
            //通过Clientsoket发送数据  
            return true;
        }
        void QuitServer()
        {
            if (serverSocket != null)
                serverSocket.Close();
            if (receiveThread != null)
                receiveThread.Abort();
            dataReceivedQueue.Clear();
        }
        delegate void CallBack(IAsyncResult ar);
        public  void ReceiveMessage_Asyn(ProcessMethod callBack)
        {
            processMethod= new ProcessMethod(callBack);
            try
            {
                // m_recvBuf = new byte[SocketParserConstants.MAX_SIZE_READ_PACKET_BUFFER];
                buffer = new byte[1024];
                EndPoint newClientEP = new IPEndPoint(IPAddress.Any, 0);
                // receive data from client
                serverSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref newClientEP, DoReceiveFrom, serverSocket);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("SocketException Start: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Start: {0}", ex.Message);
            }

        }
        private void DoReceiveFrom(IAsyncResult iar)
        {
            try
            {
                Socket recvSock = (Socket)iar.AsyncState;
                EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                int recv = recvSock.EndReceiveFrom(iar, ref clientEP);
                byte[] data = new byte[recv];
                Array.Copy(buffer, data, recv);
                EndPoint newClientEP = new IPEndPoint(IPAddress.Any, 0);
                serverSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref newClientEP, DoReceiveFrom, serverSocket);
                processMethod.BeginInvoke(data, recv, null, null);
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("Server Closing.");
            }
            catch (SocketException ex)
            {
                Console.WriteLine("SocketException DoReceiveFrom: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception DoReceiveFrom: {0}", ex.Message);
            }
        }
        public  void ReceiveMessage_ProConM()
        {
            receiveThread = new Thread(() => {
                while (true)
                {
                    try
                    {
                        int receiveNumber = serverSocket.Receive(buffer);
                        string tmp = System.Text.Encoding.ASCII.GetString(buffer, 0, receiveNumber);//接收到的数据
                        Debug.WriteLine("Reciive from {0} with {1} bytes", serverSocket.RemoteEndPoint.ToString(), tmp.Length.ToString());
                        //dataReceivedQueue.Enqueue(System.Text.Encoding.ASCII.GetBytes(string tmpstring tmp));
                        tmpbytes = System.Text.Encoding.ASCII.GetBytes(tmp);
                        modifyQueue(true);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            Debug.WriteLine(ex.Message);
                            serverSocket.Close();
                            break;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            });
        }
        private  byte [] modifyQueue(bool add)
        {
            mutex.WaitOne();
            if (add)
            {
                dataReceivedQueue.Enqueue(tmpbytes);
                mutex.ReleaseMutex();
                return null;
            }
            else
            {
                byte[] v = dataReceivedQueue.Dequeue();
                mutex.ReleaseMutex();
                return v;
            }
           
        }
        public byte [] getRecrivedData()
        {
            return modifyQueue(false);
        }
        public byte [] ReceiveMessage()
        {
            int receiveNumber = serverSocket.Receive(buffer);
            string tmp = System.Text.Encoding.ASCII.GetString(buffer, 0, receiveNumber);//接收到的数据
            Console.WriteLine("Reciive from {0}  bytes", tmp.Length.ToString());
            return System.Text.Encoding.ASCII.GetBytes(tmp);
        }
    }
}
