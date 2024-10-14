using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


public class UDPClient : MonoBehaviour
{
    Socket socket; //目标socket
    EndPoint serverEnd; //服务端
    IPEndPoint ipEnd; //服务端端口
    string recvStr; //接收的字符串
    string sendStr; //发送的字符串
    byte[] recvData = new byte[2048]; //接收的数据，必须为字节
    byte[] sendData = new byte[2048]; //发送的数据，必须为字节
    int recvLen=0; //接收的数据长度
    Thread connectThread; //连接线程

    void Start()
    {
        InitSocket(); //在这里初始化
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            print("已经发送");
            SocketSend("我发的只是文字信息Benny0119");
        }
    }

    //初始化
    void InitSocket()
    {
        //定义连接的服务器ip和端口，可以是本机ip，局域网，互联网
        ipEnd = new IPEndPoint(IPAddress.Parse("192.168.1.159"), 7788);
        //定义套接字类型,在主线程中定义
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //定义服务端
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        serverEnd = (EndPoint)sender;
        print("等待发送UDP dgram");

        //建立初始连接，这句非常重要，第一次连接初始化了serverEnd后面才能收到消息
        SocketSend("UDP已连接—测试接收中文+CeShi+3333");
      
        //开启一个线程连接，必须的，否则主线程卡死
        connectThread = new Thread(new ThreadStart(SocketReceive));
        connectThread.Start();
    }

    void SocketSend(string sendStr)
    {
        //清空发送缓存
        sendData = new byte[2048];
        //数据类型转换
        sendData = Encoding.UTF8.GetBytes(sendStr);
        //发送给指定服务端
        socket.SendTo(sendData, ipEnd);
    }

    //服务器接收
    void SocketReceive()
    {
        //进入接收循环
        while (true)
        {
            //对data清零
            recvData = new byte[2048];
            //获取客户端，获取服务端端数据，用引用给服务端赋值，实际上服务端已经定义好并不需要赋值
            
            recvLen = socket.ReceiveFrom(recvData, ref serverEnd);

            print("信息来自: " + serverEnd.ToString()); //打印服务端信息//输出接收到的数据
            recvStr = Encoding.UTF8.GetString(recvData, 0, recvLen);
            print("我是客户端，接收到服务器的数据" + recvStr);
        }
    }

    //连接关闭
    void SocketQuit()
    {
        //关闭线程
        if (connectThread != null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
        //最后关闭socket
        if (socket != null)
            socket.Close();
    }

    void OnApplicationQuit()
    {
        SocketQuit();
    }
}
