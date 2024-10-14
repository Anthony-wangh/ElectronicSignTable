using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
public class SendImageScript : MonoBehaviour
{

    byte[] imagebytes;

    string newImageString;

    private void Start()
    {
        NewUDPClient.instance.ReSendIndexEvent += GetReSendIndexFromUDPClient;
        EventManager.Instance.AddListener("SendSignToServer", Send);
    }


    private void Send(object sender, EventArgs e)
    {
        ClientEventArgs args = (ClientEventArgs)e;
        if (args != null)
        {
            if (string.IsNullOrEmpty(args.TexturePath))
                return;
            Send(args.TexturePath);
        }
    }

    private void Send(string path) {

        FileStream files = new FileStream(path, FileMode.Open);
        imagebytes = new byte[files.Length];
        files.Read(imagebytes, 0, imagebytes.Length);
        files.Close();
        picStr = Convert.ToBase64String(imagebytes);
        StartCoroutine(SendPicture());
    }

    string picStr;
    IEnumerator SendPicture()
    {
        UDPSplit(picStr);
        //将图片发送给投影机
        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < num - 1000; i++)
        {
            if (UDPStringDic.TryGetValue(i, out newImageString))
            {
                NewUDPClient.instance.SocketSend(newImageString);
            }
        }

        yield return new WaitForSeconds(0.1f);
        //发送完成后发一条信息给服务端
        NewUDPClient.instance.SocketSend("这是图片");
    }

    string[] reSendNum;
    int newindex;
    void GetReSendIndexFromUDPClient(string str)
    {
        reSendNum = str.Split('_');

        for (int i = 0; i < reSendNum.Length; i++)
        {
            if (int.TryParse(reSendNum[i], out newindex))
            {
                if (UDPStringDic.TryGetValue(newindex, out newImageString))
                {
                    NewUDPClient.instance.SocketSend(newImageString);
                }
            }
        }
        //发送完成后发一条信息给服务端
        NewUDPClient.instance.SocketSend("这是图片");
        print("重新发送完毕");

    }



    Dictionary<int, string> UDPStringDic = new Dictionary<int, string>();
    int index = 0;
    int maxIndex = 1000;
    string newstr;
    int num;

    void UDPSplit(string str)
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


    public static long GetTimeStamp(bool bflag = true)
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        long ret;
        if (bflag)
            ret = Convert.ToInt64(ts.TotalSeconds);
        else
            ret = Convert.ToInt64(ts.TotalMilliseconds);
        return ret;
    }

    private void OnDisable()
    {
        NewUDPClient.instance.ReSendIndexEvent -= GetReSendIndexFromUDPClient;
    }
}
