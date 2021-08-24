using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using UnityEngine;

public class log_wm : MonoBehaviour
{
    private static string logName;
    private static string logPath;
    private static string mainPath = Directory.GetParent(Application.dataPath).ToString();
    private static Queue<string> messages = new Queue<string>();//消息队列
    private static bool istuichuXiancheng = true;//是否退出线程
    private static object locker = new object();
    public static bool isLog = true;//是否用日志
    public static int maxNumber=9;
    /// <summary>
    /// 主目录
    /// </summary>
    private static string mainDirectory = "";

    /// <summary>
    /// 日志索引
    /// </summary>
    private static int logIndex = 0;

    /// <summary>
    /// 日志文件大小
    /// </summary>
    private static long fileSize = 0;
    //public static bool isXintiao = false;//是否输出心跳

    /// <summary>
    /// 主目录
    /// </summary>
    private static string MainDirectory
    {
        get
        {
            if (mainDirectory.Equals(""))
            {
                mainDirectory = mainPath + "/wmLog";
                DirectoryInfo mydir = new DirectoryInfo(mainDirectory);
                if (!mydir.Exists)
                {
                    Directory.CreateDirectory(mainDirectory);
                }
            }
            return mainDirectory;

        }
    }

    public static string LogName
    {
        get
        {
            if (logName == null)
            {
                logName = "server-" + System.DateTime.Now.ToString("yyyy-MM-dd") + "-" + logIndex;
            }
            return logName;
        }
        set
        {
            logName = value;
        }
    }
    public static string LogPath//日志路径
    {
        get
        {
            if (string.IsNullOrEmpty(logPath))
            {
                logPath = MainDirectory + "/" + LogName + ".txt";
            }
            return logPath;
        }
        set
        {
            logPath = value;
        }
    }


    /// <summary>
    /// 日志索引
    /// </summary>
    private static int LogIndex
    {
        set
        {
            logIndex = value;
            LogName = "server-" + System.DateTime.Now.ToString("yyyy-MM-dd") + "-" + logIndex;
            LogPath = MainDirectory + "/" + LogName + ".txt";
        }
        get
        {
            return logIndex;
        }
    }

    public static void wmlog(string str)//输出日志str
    {
        if (!isLog)
        {
            return;
        }
        if (str.Contains("心跳"))
        {
            return;
        }
        lock (messages)
        {
            messages.Enqueue(str);
        }

        lock (locker)
        {
            if (istuichuXiancheng)
            {
                istuichuXiancheng = false;
                Thread _Thread = new Thread(listeningData);
                _Thread.Start();
            }
        }
    }

    private static void writeDataToLog(string str)//写入日志数据
    {
        if (!File.Exists(LogPath))
        {
            var temp = File.Create(LogPath);
            temp.Close();
        }
        else
        {
            fileSize = GetFileLength(LogPath);
            while (fileSize > 1024)
            {
                LogIndex++;
                if (File.Exists(LogPath))
                {
                    fileSize = GetFileLength(LogPath);
                }
                else
                {
                    var temp = File.Create(LogPath);
                    temp.Close();
                    break;
                }
            }
        }


        FileStream fs = new FileStream(LogPath, FileMode.Append);
        StreamWriter sw = new StreamWriter(fs);
        sw.WriteLine(str + "——" + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"));
        sw.Close();
        fs.Close();
        DelLog();
    }

    private static void listeningData()
    {
        string currentMeg = "";
        while (messages.Count > 0)
        {
            try
            {
                lock (messages)
                {
                    currentMeg = messages.Dequeue();
                }
                if (currentMeg.Equals(""))
                {
                    continue;
                }
                writeDataToLog(currentMeg);
            }
            catch
            {
                continue;
            }
        }
        istuichuXiancheng = true;
    }

    /// <summary>
    /// 获取指定文件大小
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>21504/1048576f>20
    public static long GetFileLength(string filePath)
    {
        //判断当前路径所指向的是否为文件
        if (File.Exists(filePath))
        {
            //定义一个FileInfo对象,使之与filePath所指向的文件向关联,
            //以获取其大小
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.Length;
        }
        else
        {
            return -1;
        }
    }
    private static void DelLog()
    {
        string path = string.Format("{0}", MainDirectory);
        if (Directory.Exists(path))
        {
            DateTime dateTime = DateTime.Now;
            string time = dateTime.ToShortDateString().ToString();
            string[] str2 = time.Split('/');
            DirectoryInfo direction = new DirectoryInfo(path);
            FileInfo[] files = direction.GetFiles("*");
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.EndsWith(".txt"))
                {
                   
                    if (files.Length > 10)
                    {
                        string[] str = files[i].Name.Split('-');

                        string[] str1 = str[4].Split('.');
                        int j = int.Parse(str1[0]);
                        //Debug.Log(j);
                        if (j < LogIndex - maxNumber)
                        {
                            if (int.Parse(str2[2]) == int.Parse((str[3])))
                            {
                                File.Delete(files[i].FullName);
                            }
                        }
                    }
                }
            }
        }
    }
}
