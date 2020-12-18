using SocketEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SocketEngine.UdpSocketListener;

namespace SocketTool
{
    class Program
    {
        static void Main(string[] args)
        {
            //使用服务器方法1
            SocketServer_udp socketServer_Udp1 = new SocketServer_udp();
            socketServer_Udp1.InitServer(2222);
            socketServer_Udp1.ReceiveMessage_Asyn(printrec);//每次服务器收到数据都会调用printrec 可以自己定义一个 static public void  函数名(byte [] bs,int len) 这种类型的函数来处理收到的数据
           
            //使用服务器方法2
            SocketServer_udp socketServer_Udp2 = new SocketServer_udp();
            socketServer_Udp2.InitServer(2222);
            Console.WriteLine(BitConverter.ToString(socketServer_Udp2.ReceiveMessage()));//执行socketServer_Udp2.ReceiveMessage()后会等待数据报到来 这个函数其实是socket.receive()

            //使用服务器方法3
            SocketServer_udp socketServer_Udp3 = new SocketServer_udp();
            socketServer_Udp3.InitServer(2222);
            socketServer_Udp3.ReceiveMessage_ProConM();//这时会开启线程接收数据
            //这时可以做其他事情
            TimeSpan interval = new TimeSpan(0, 0, 2);
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine("doing other works for 2 sec...");
                Thread.Sleep(interval);
            }
            //需要数据时可以调用一下getRecrivedData()，每调用一次获取数据报缓存队列的队头元素
            byte[] bytes;
            while((bytes= socketServer_Udp3.getRecrivedData())!=null)
            {
                Console.WriteLine(BitConverter.ToString(bytes));
            }

            //使用客户端方法
            string s = "123456789";
            SocketClinet_udp socketClinet_Udp = new SocketClinet_udp("127.0.0.1", 2222);
            socketClinet_Udp.Send(System.Text.Encoding.ASCII.GetBytes(s));

        }
       static public void  printrec(byte [] bs,int len)
        {
            Console.WriteLine("printrec run");
            Console.WriteLine(BitConverter.ToString(bs));
        }
    }
}
