using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 日志存储助手
/// </summary>
public class FileLog
{
    /// <summary>
    /// 是否开启日志
    /// </summary>
    public static bool IsOn = true;

    /// <summary>
    /// 日志保留天数
    /// </summary>
    public int SaveDay = 1;

    /// <summary>
    /// 单个文件大小
    /// </summary>
    public int MaxSize = 2000; //kb

    /// <summary>
    /// 是否可以写入日志
    /// </summary>
    private bool mIsCanWrite = false;

    /// <summary>
    /// 日志缓冲
    /// </summary>
    private Dictionary<string,string> buffer = new Dictionary<string,string>();

    /// <summary>
    /// 日志级别
    /// </summary>
    public enum Level
    { 
        I, //INFO
        W, //WARNING
        E, //ERROR
        V, //VERBOSE
        D, //DEBUG
    }

    /// <summary>
    /// 获取当前平台下的存储路径
    /// </summary>
    private string mPath;
    public string BasePath
    {
        get 
        {
            if(mPath == null)
            {
                switch(Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.WindowsPlayer:
                        mPath = Application.dataPath + "/../Log/";
                        break;
                    case RuntimePlatform.IPhonePlayer:
                        mPath = Application.persistentDataPath + "/Log/";
                        break;
                    case RuntimePlatform.Android:
                        mPath = "/sdcard/.javgame/Log/";
                        break;
                }
            }
            return mPath;
        }
    }


    /// <summary>
    /// 日志名称
    /// </summary>
    private string _mLogName;
    private string mLogName
    {
        get
        {
            if(_mLogName == null)
            {
                _mLogName = DateTime.Now.ToString("(HH-mm)") + ".html";
            }
            return _mLogName;
        }
    }

    /// <summary>
    /// 日志路径
    /// </summary>
    private string mLogPath
    {
        get
        {
            return BasePath + DateTime.Now.ToString("yyyy-MM-dd")+"/";
        }
    }

    /// <summary>
    /// 构造函数
    /// 创建需要的文件夹和删除过期日志
    /// </summary>
    public FileLog()
    {
        Config();

        if (IsOn)
        {
            CreateLogFile();
            DeleteOldLog();
        }
        else
        {
            mIsCanWrite = false;
        }
    }

    /// <summary>
    /// 根据不同平台进行默认日志配置
    /// </summary>
    public void Config()
    { 
        if(Application.platform == RuntimePlatform.Android
            ||Application.platform == RuntimePlatform.IPhonePlayer)
        {
            //在手机平台默认不开启日志，通过手动开启
            IsOn = Directory.Exists(BasePath);
        }
#if UNITY_WEBPLAYER
        IsOn = false;
#endif
    }

    /// <summary>
    /// 析构函数
    /// 这个执行的时机不确定
    /// Editer中第二次运行才会执行
    /// </summary>
    ~FileLog()
    {
        End();
    }

    /// <summary>
    /// 写入日志尾部结束写日志
    /// </summary>
    public void End()
    {
        mIsCanWrite = false;//END后不让再写入日志
        WriteBuffer(HtmlEnd);
    }

    /// <summary>
    /// 缓冲区的日志写入文件
    /// </summary>
    public void WriteBuffer(string str = "")
    {
        if(buffer.Count == 0)
        {
            return;
        }

        //数组在变，只能copy一份来写入
        string[] keys = new string[buffer.Keys.Count];
        buffer.Keys.CopyTo(keys,0);

        foreach (string key in keys)
        {
            buffer[key] += str;
            Write(key);
        }
        buffer.Clear();
    }

    /// <summary>
    /// 记录日志
    /// </summary>
    /// <param name="prefix">文件前缀</param>
    /// <param name="log">日志内容</param>
    private void Add(string prefix,Level level,string from, string tag , string log)
    {
        if(!mIsCanWrite)
        {
            return;
        }

        FileLogListen.CheckListen(); //创建日志监听

        log = Format(level,from,tag,log);

        if(!buffer.ContainsKey(prefix))
        {
            buffer[prefix] = "";
        }

        buffer[prefix] += log;

        if(buffer[prefix].Length < 10000)
        {
            return;
        }

        Write(prefix);
    }

    /// <summary>
    /// 写日志
    /// </summary>
    private void Write(string prefix)
    {
#if !UNITY_WEBPLAYER
        try
        {
            string path = mLogPath + "/" + prefix + mLogName;
            FileInfo info = new FileInfo(path);
            if (!info.Exists)
            {
                buffer[prefix] = HtmlHead + buffer[prefix]; //加入头部
            }
            else if(info.Length / 1024 >= MaxSize)
            {
                _mLogName = null;
                path = mLogPath + "/" + prefix + mLogName;
                buffer[prefix] = HtmlHead + buffer[prefix]; //加入头部
            }
            File.AppendAllText(path, buffer[prefix]);
            buffer[prefix] = "";
        }
        catch (Exception e)
        {
            Debug.LogError("WriteLog:" + e.Message);
        }
#endif
    }

    /// <summary>
    /// 格式化日志
    /// </summary>
    private string Format(Level level,string from, string tag , string log)
    {
        string data = "<div class='item'>\r\n";
        data += "<div class='time'>"+DateTime.Now.ToString("HH:mm:ss:ffff") + "</div>\r\n";
        data += "<div class='level'>" + level + "</div>\r\n";
        data += "<div class='tag'>" + tag + "</div>\r\n";
        data += "<div class='content'>" + log + "</div>\r\n";
        data += "<div class='from'>" + from + "</div>\r\n";
        data += "</div>\r\n";
        return data;
    }

    /// <summary>
    /// 创建日志文件夹
    /// </summary>
    private void CreateLogFile()
    { 
        if(BasePath == null)
        {
            mIsCanWrite = false;
            return;
        }
        try
        {
            if (!Directory.Exists(mLogPath))
            {
                Directory.CreateDirectory(mLogPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("CreateLogFolder:" + e.Message);
        }
        finally
        {
            mIsCanWrite = Directory.Exists(mLogPath);
        }
    }

    /// <summary>
    /// 删除过期日志
    /// </summary>
    private void DeleteOldLog()
    { 
#if !UNITY_WEBPLAYER
        if(!mIsCanWrite)
        {
            return;
        }
        try
        {
            foreach (string name in Directory.GetDirectories(mPath))
            {
                DateTime createTime = Directory.GetCreationTime(name);
                if(createTime.Month != DateTime.Now.Month || (DateTime.Now.Day - createTime.Day) > SaveDay)
                {
                    Directory.Delete(name, true);
                }
            }
        }
        catch(Exception e)
        {
            Debug.LogError("DeleteOldLog:"+e.Message);
        }
#endif
    }

    #region 静态方法
    /// <summary>
    /// 内部实例
    /// </summary>
    private static FileLog mFlog;
    public static FileLog FLog
    {
        get
        {
            if (mFlog == null)
            {
                mFlog = new FileLog();
            }
            return mFlog;
        }
    }

    /// <summary>
    /// 获取文件来源信息
    /// </summary>
    public static string FromInfo
    {
        get
        {
            return GetFromInfo(new System.Diagnostics.StackTrace(1, true));
        }
    }

    /// <summary>
    /// 获取日志来源信息
    /// </summary>
    public static string GetFromInfo(System.Diagnostics.StackTrace ST)
    {
        System.Diagnostics.StackFrame frame = ST.GetFrame(0);
        string fileName = Path.GetFileName(frame.GetFileName());
        return "[FILE:" +fileName + "][METHOD:" + frame.GetMethod() + "][LINE:" + frame.GetFileLineNumber()+"]";
    }
    #endregion

    #region 日志记录方法
    public static void Log(string log)
    {
        FLog.Add("LOG", Level.I, GetFromInfo(new System.Diagnostics.StackTrace(1, true)), "NOTAG", log);
    }

    public static void Log(string tag, string log)
    {
        FLog.Add("LOG", Level.I, GetFromInfo(new System.Diagnostics.StackTrace(1, true)), tag, log);
    }

    public static void Log(Level level, string log)
    {
        FLog.Add("LOG", level, GetFromInfo(new System.Diagnostics.StackTrace(1, true)), "NOTAG", log);
    }

    public static void Log(Level level, string tag, string log)
    {
        FLog.Add("LOG", level, GetFromInfo(new System.Diagnostics.StackTrace(1, true)), tag, log);
    }

    public static void Log(string prefix, Level level, string log)
    {
        FLog.Add(prefix, level, GetFromInfo(new System.Diagnostics.StackTrace(1, true)), "NOTAG", log);
    }

    public static void Log(string prefix, string tag, string log)
    {
        FLog.Add(prefix, Level.I, GetFromInfo(new System.Diagnostics.StackTrace(1, true)), tag, log);
    }

    public static void Log(string prefix, Level level, string log, string from)
    {
        FLog.Add(prefix, level, from, "NOTAG", log);
    }

    public static void Log(string prefix, Level level, string tag, string log, string from)
    {
        FLog.Add(prefix, level, from, tag, log);
    }
    #endregion

    #region HTML
    private const string HtmlHead 
    = @"
<html>
    <head>
        <title>日志</title>
        <style type='text/css'>
            html,body
	        {
		        height: 100%; margin: 0 auto; padding: 0; font-family:tahoma;
	        }
	        .item,.time,.tag,.level,.content,.from
	        {
		        float:left;
	        }
	        .item
	        {
		        width:100%;
		        background-color: #EFF7FF;
				border-bottom:20px solid #F7F7FF;
	        }
	        .item:hover
	        {
		        background-color: #F8B3D0;
	        }
	        .time
	        {
		        width:8%;
	        }
	        .tag
	        {
		        width:10%;
	        }
	        .level
	        {
		        width:2%;
		        text-align:center ;
	        }
	        .content
	        {
		        width:60%;
	        }
	        .from
	        {
		        width:20%;
	        }
        </style>
        <meta http-equiv='content-type' content='text/html; charset=UTF-8' />
    </head>
    <body>
    ";

    private const string HtmlEnd
    = @"
            </body>
        </html>
    ";
    #endregion
}

#region 日志监听器，确保日志记录不丢失
public class FileLogListen : MonoBehaviour
{
    private static GameObject mSelf;

    /// <summary>
    /// 检测日志监听器是否存在
    /// </summary>
    public static void CheckListen()
    {
        if (mSelf == null)
        {
            mSelf = new GameObject("FileLogListen", typeof(FileLogListen));
        }
    }

    void Start()
    {
        mSelf = gameObject;
    }

    /// <summary>
    /// 监听器被销毁写入当前日志
    /// </summary>
    void OnDestroy()
    {
        FileLog.FLog.WriteBuffer();
    }

    /// <summary>
    /// 检测到退出写入日志结尾
    /// </summary>
    void OnApplicationQuit()
    {
        FileLog.FLog.End();
    }
}
#endregion