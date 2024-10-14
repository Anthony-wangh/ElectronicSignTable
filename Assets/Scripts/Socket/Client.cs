using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;


public class ClientEventArgs : EventArgs { 

    public Texture2D Texture;
    public string TexturePath;
}

public class Client : MonoBehaviour
{
    public string IP = "";
    public int Port = 8001;
    private Socket clientSocket;
    private byte[] buffer = new byte[60000];

    void Start()
    {
        EventManager.Instance.AddListener("SendSignToServer", Send);
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        clientSocket.BeginConnect(IP, Port, ConnectCallback, null);
    }

    private void Send(object sender, EventArgs e)
    {
        ClientEventArgs args = (ClientEventArgs)e;
        if (args != null)
        {
            if (args.Texture == null)
                return;
            buffer = args.Texture.EncodeToPNG();
            clientSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallback, null);
        }       
    }

    private void ConnectCallback(System.IAsyncResult ar)
    {
        clientSocket.EndConnect(ar);
        Debug.Log("Connected to server.");        
    }


    private void SendCallback(System.IAsyncResult ar)
    {
        int sent = clientSocket.EndSend(ar);
        Debug.Log("Sent: " + sent + " bytes.");
    }



    Dictionary<int, string> UDPStringDic = new Dictionary<int, string>();
    int index = 0;
    int maxIndex = 1000;
    string newstr;
    int num;

    //拆包发送
    void PackegeSplit(string str)
    {
        index = 0;
        maxIndex = 1000;
        int stringTag = 1000;
        UDPStringDic.Clear();
        num = (str.Length / 1000) + 1 + 1000;   //将数字变成四位数的，三个字节
                                                //  print(num-1000);
        for (int i = 0; i < num - 1000; i++)
        {
            if (maxIndex > str.Length - index)
            {
                maxIndex = str.Length - index;
            }
            newstr = "1551683020" + "_" + num + "_" + stringTag + "_" + str.Substring(index, maxIndex); //包名，包长，包的顺序号，包的内容


            UDPStringDic.Add(stringTag - 1000, newstr);
            stringTag++;
            index += 1000;
        }
    }

}