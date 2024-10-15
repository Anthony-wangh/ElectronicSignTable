using UnityEngine;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;

public class NewUDPClient : MonoBehaviour
{

    public static NewUDPClient instance;
    //服务端的IP
    private string UDPClientIP;
    //目标socket 
    Socket socket;
    //服务端 
    EndPoint serverEnd;
    //服务端端口 
    IPEndPoint ipEnd;
    //接收的字符串 
    string recvStr;
    //接收的数据，必须为字节 
    byte[] recvData;
    //发送的数据，必须为字节 
    byte[] sendData;
    //接收的数据长度 
    int recvLen = 0;
    //连接线程
    Thread connectThread;

    bool isClientActive = false;

    //连接服务器时发送的vector3类型
    Vector3 startVec = Vector3.zero;

    bool isStartHeart = false;


    public int port;

    //判断是否让客户端重新发送数据包
    bool isReSend = false;
    string reSendStrIndex;

    public delegate void ReSendIndexDeledate(string str);
    public event ReSendIndexDeledate ReSendIndexEvent;


    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        UDPClientIP = GetPlayerIp();//服务端的IP.自己更改
        UnityEngine.Debug.Log(UDPClientIP);
        //port = 7788;
        UDPClientIP = UDPClientIP.Trim();
        isClientActive = true;
        InitSocket(); //在这里初始化
    }


    /// <summary>
    /// 获取本机IP
    /// </summary>
    /// <returns>string :ip地址</returns>
        public static string GetPlayerIp()
   {        
 
        IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
        for (int i = 0; i<ips.Length; i++)
        {
            IPAddress address = ips[i];
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                return address.ToString();//返回ipv4的地址的字符串
            }
        }
        //找不到就返回本地
        return "127.0.0.1";
    }

    private void Update()
    {
        if (isStartHeart)
        {
            HeartSend();
        }
        //检测心跳与心跳反馈的间隔时间，
        timerInterval += Time.deltaTime;

        if (timerInterval > 6f)
        {
            print("连接异常");
            timerInterval = 0f;
        }

        if (isReSend)
        {
            if (ReSendIndexEvent != null)
            {
                ReSendIndexEvent(reSendStrIndex);
                reSendStrIndex = null;
                isReSend = false;
            }
        }
    }

    //初始化 
    void InitSocket()
    {
        //定义连接的服务器ip和端口，可以是本机ip，局域网，互联网 
        ipEnd = new IPEndPoint(IPAddress.Parse(UDPClientIP), port);
        //定义套接字类型,在主线程中定义 
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //socket.Bind(ipEnd);
        //定义服务端 
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        serverEnd = (EndPoint)sender;
        print("local：等待连接");
        isStartHeart = true;
        //开始心跳监听
        //客户端发送心跳消息后，计时器开始计时，判断3秒内是否能收到服务端的反馈
        HeartSend();
        //开启一个线程连接，否则主线程卡死 
        connectThread = new Thread(new ThreadStart(SocketReceive));
        connectThread.Start();
    }

    //发送字符串
    public void SocketSend(string sendStr)
    {
        //清空发送缓存 
        sendData = new byte[1500];
        //数据类型转换 
        sendData = Encoding.UTF8.GetBytes(sendStr);
        //发送给指定服务端
        socket.SendTo(sendData, sendData.Length, SocketFlags.None, ipEnd);
    }

    //发送消息频率
    float timerRate = 5;
    //接收服务端心跳反馈的时间间隔
    float timerInterval = 0f;

    byte[] heartSendData = new byte[1024];

    /// <summary>
    /// 心跳
    /// </summary>
    void HeartSend()
    {
        timerRate += Time.deltaTime;
        if (timerRate > 5f)
        {
            try
            {
                SocketSend("alive");
            }
            catch
            {
            }
            timerRate = 0f;
        }
    }

    //客户端接收服务器消息
    void SocketReceive()
    {
        //进入接收循环 
        while (isClientActive)
        {
            //对data清零 
            recvData = new byte[20000];
            try
            {
                //获取服务端端数据
                recvLen = socket.ReceiveFrom(recvData, ref serverEnd);
                if (isClientActive == false)
                {
                    break;
                }
            }
            catch
            {

            }
            //输出接收到的数据 
            if (recvLen > 0)
            {
                //接收到的信息
                recvStr = Encoding.UTF8.GetString(recvData, 0, recvLen);
            }
            print("server：" + recvStr);

            //心跳回馈
            if (recvStr == "keeping")
            {
                // 当服务端收到客户端发送的alive消息时
                print("连接正常");
                timerInterval = 0;
            }
            else if (recvStr != null)
            {
                reSendStrIndex = recvStr;
                isReSend = true;
            }
        }
    }

    //连接关闭
    void SocketQuit()
    {
        //最后关闭socket
        if (socket != null)
            socket.Close();
    }
    void OnApplicationQuit()
    {
        isStartHeart = false;
        isClientActive = false;
        SocketQuit();
        Thread.Sleep(25);
    }
}
