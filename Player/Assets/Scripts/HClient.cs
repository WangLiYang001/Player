using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class HClient : MonoBehaviour
{
    public static HClient Instance;
    public Socket clientSocket;
    public string Ip = "192.168.1.136";
    public int port = 8000;
    public string jieShouMessage;//接收到的服务器消息
    IPAddress ip;
    public float duanXianChongLianTime = 1;
    public float currentchonglianshijian;
    private static byte[] result = new byte[1024 * 1024 * 3];
    Thread c_thread;
    public Queue<string> jieShouMessages = new Queue<string>(); //接收到的消息放到这个队列中
    public delegate void chuLiXiaoXi(string message);//处理消息委托
    public event chuLiXiaoXi event_chuLiXiaoXi;//处理消息事件
    public delegate void lianJieChengGongChuFa();//连接成功后委托
    public event lianJieChengGongChuFa event_lianJieChengGongChuFa;//连接成功之后处理事件
    public string linshistring = "";//临时存放一个数据的变量
    private bool isReceive = true;//是否接收，作为接收数据死循环的退出条件
    private bool isDuiFangDuanKai = false;//对方是否断开连接
    Configuration _configuration = null;
    [Serializable]
    public class Configuration
    {
        public string _ip;
        public string _port;
        public string duanXianChongLianTime;
    }
  
    public Configuration configuration
    {
        get { return _configuration; }
    }
    private void Awake()
    {
        Instance = this;
        string fullPath = Application.streamingAssetsPath + "/" + "环境配置.json";
        log_wm.wmlog(fullPath);
        string str = File.ReadAllText(fullPath);
        if (string.IsNullOrEmpty(str) == false)
        {
            _configuration = JsonUtility.FromJson<Configuration>(str);
        }
        Ip = _configuration._ip;
        port = (int)(float.Parse(_configuration._port));
        duanXianChongLianTime = (int)(float.Parse(_configuration.duanXianChongLianTime));
    }
    
    private void Start()
    {
        //connectServer();
        currentchonglianshijian = duanXianChongLianTime;
        log_wm.wmlog("start");


    }
    public void fengeMessageToQueue2(string message)
    {
        foreach (char zifu in message)
        {
            if (zifu.Equals('*'))
            {
                jieShouMessages.Enqueue(linshistring);
                linshistring = "";
                continue;
            }
            linshistring += zifu;

        }
    }
    string lsstr;
    void Update()
    {
        if ((clientSocket == null || (clientSocket != null && (!clientSocket.Connected || isDuiFangDuanKai))) && currentchonglianshijian < 0)
        {
            
            connectServer();
            currentchonglianshijian = duanXianChongLianTime;
        }
        currentchonglianshijian -= Time.deltaTime;
        for (int i = 0; i < 20; ++i)
        {
            if (jieShouMessages.Count > 0)
            {
                lsstr = jieShouMessages.Dequeue();
                event_chuLiXiaoXi?.Invoke(lsstr);
                //if (event_chuLiXiaoXi != null)
                //{
                //    event_chuLiXiaoXi(lsstr);
                //}
            }
            else
            {
                break;
            }
        }


    }

    public void jieShouServerString()
    {//接受到的服务器消息

        //通过clientSocket接收数据
        do
        {
            try
            {
                int receiveLength = clientSocket.Receive(result);

                if (receiveLength == 0)
                {
                    UnityEngine.Debug.LogError("对方断开连接,当前socket状态==" + clientSocket.Connected);
                    isDuiFangDuanKai = true;
                    break;
                }
                jieShouMessage = Encoding.UTF8.GetString(result, 0, receiveLength);
             
                fengeMessageToQueue2(jieShouMessage);
                //jieShouMessages.Enqueue(jieShouMessage);
            }
            catch (ThreadAbortException ex)
            {

            }
            catch (Exception e)
            {
                isDuiFangDuanKai = true;
                break;
            }

        }
        while (isReceive);



    }

    /// <summary>
    /// 连接当前ip和端口的服务器
    /// </summary>
    private void connectServer()
    {

        ip = IPAddress.Parse(Ip);

        try
        {
            log_wm.wmlog("connectServer");
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            log_wm.wmlog("ip="+ ip+":"+port);
            clientSocket.Connect(new IPEndPoint(ip, port)); //配置服务器IP与端口
            log_wm.wmlog("connectServer1");
            //print(clientSocket.RemoteEndPoint);
            event_lianJieChengGongChuFa?.Invoke();

            log_wm.wmlog("连接服务器成功" + ip + ":" + port);
            isDuiFangDuanKai = false;
            if (c_thread != null && c_thread.IsAlive)
            {
                c_thread.Abort();
            }
            isReceive = true;
            c_thread = new Thread(jieShouServerString);
            c_thread.Start();
            //print("连接成功时，socket="+clientSocket);
        }
        catch (Exception e)
        {
            log_wm.wmlog("连接失败:" + ip + ":" + port + e.Message);
            isDuiFangDuanKai = true;
        }
    }



    public void sendMessageToServer(string message)
    {//向服务器发送消息
        try
        {
            clientSocket.Send(Encoding.UTF8.GetBytes(message + "*"));//给服务器发消息的时候末尾加*号表示结束标志
            //log_wm.wmlog("→" + message);
        }
        catch (SocketException se)
        {

            duankailianjie();
            //log_wm.wmlog(message+"=消息发送失败，重新连接"+ se);
            isDuiFangDuanKai = true;

        }
        catch (Exception ex)
        {
            //log_wm.wmlog("发送出错:"+ message+"="+ex);
        }
    }

    public void fasong()
    {
        sendMessageToServer("我是客户端");
    }

    public void lianjie()
    {
        duankailianjie();
    }


    /// <summary>
    /// 安全关闭socket
    /// </summary>
    /// <param name="socket"></param>
    private static void SafeClose(Socket socket)
    {
        if (socket == null)
            return;

        //if (!socket.Connected)
        //    return;

        try
        {
            socket.Shutdown(SocketShutdown.Both);
        }
        catch
        {
        }

        try
        {
            socket.Close();
        }
        catch
        {
        }
    }


    void OnApplicationQuit()
    {
        duankailianjie();
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    private void duankailianjie()
    {//断开连接
        isReceive = false;
        //yield return new WaitForSeconds(1);
        try

        {
            if (c_thread != null && c_thread.IsAlive)
                c_thread.Abort();
        }
        catch (Exception ex)
        {
            print(ex);
        }

        SafeClose(clientSocket);

    }



    /// <summary>
    /// 赋值ip和port，断开已有连接，自动创建新的连接
    /// </summary>
    /// <param name="waibuip">ip</param>
    /// <param name="waibuport">port</param>
    public void connectServer(string waibuip, int waibuport)
    {
        Ip = waibuip;
        port = waibuport;
        duankailianjie();
    }
   

    public void Exit()
    {
        Application.Quit();
        duankailianjie();
    }
    
}
