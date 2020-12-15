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
        private  void ReceiveMessage_Asyn(CallBack callBack)
        {
            if (receiveThread != null)
            {
                receiveThread.Abort();
                dataReceivedQueue.Clear();
            }              
            try
            {
                //  serverSocket.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(callBack), buffer);
             //   serverSocket.BeginReceive();
            }
            catch (Exception ex)
            {
                try
                {
                    Debug.WriteLine(ex.Message);
                   // serverSocket.EndReceive();
                    serverSocket.Close();
                   // break;
                }
                catch (Exception)
                {
                }
            }
            
        }

        private  void ReceiveMessage_ProConM()
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
            Debug.WriteLine("Reciive from {0} with {1} bytes", serverSocket.RemoteEndPoint.ToString(), tmp.Length.ToString());
            return System.Text.Encoding.ASCII.GetBytes(tmp);
        }
    }
}
