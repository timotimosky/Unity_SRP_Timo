using System;
using System.IO;
using System.Text;
using UnityEngine;
/// <summary>
/// 异常处理:
/// 1.日志记录异常上报,我们只上报我们关注的关键堆栈
/// 2.当异常发生，可能内存出错，我们无法通过代码来解决所有问题。所以需要程序停止，避免出现意外：比如给玩家增加或者减少了充值/金币
/// 并不是任何时候我们都要保证程序不崩溃。出异常并且不能解决的情况下，反而不应该让程序继续运行
/// </summary>
public class HandleLog
{
    // Start is called before the first frame update
    static HandleLog()
    {
        Application.logMessageReceived -= LogHandle;
       // Application.logMessageReceived += LogHandle; //保证项目中只有一个Application.RegisterLogCallback注册回调，否则后面注册的会覆盖前面注册的回调！
        Application.logMessageReceivedThreaded += DebugTtyCatchThreaded; //在一个新的线程中调用委托
        Debug.Log("open log write: dir is " + LogFileDir);
        OpenDirectory(LogFileDir, false);

    }
    public static bool ifClean =true;
    public static bool EnableStack = false;
    public static StreamWriter LogFileWriter = null;
    public static string LogFileName = DebugTool.WriteKey+ ".txt";

    public static string LogFileDir = Application.persistentDataPath + "/"+ DebugTool.WriteKey+"/";

    public static void OpenDirectory(string path, bool isFile = false)
    {
        if (string.IsNullOrEmpty(path)) return;
        path = path.Replace("/", "\\");
        if (isFile)
        {
            if (!File.Exists(path))
            {
                Debug.LogError("No File: " + path);
                return;
            }
            path = string.Format("/Select, {0}", path);
        }
        else
        {
            if (!Directory.Exists(path))
            {
                Debug.LogError("No Directory: " + path);
                return;
            }
        }
        //可能360不信任
        System.Diagnostics.Process.Start("explorer.exe", path);
    }

    private static void LogHandle(string logString, string stackTrace, LogType type)
    {
        if (!DebugTool.EnableSave)
        {
            return;
        }

        if (logString == null || logString.Equals("") || logString.StartsWith(DebugTool.DontWriteKey))
        {
            return;
        }
        if (!logString.Contains(DebugTool.WriteKeyString)) //我们可以允许只输出使用带有LocateString标志打印的日志
        {
            return;
        }
        StringBuilder sb = new StringBuilder();
        sb.Append(logString);
        sb.Append("\n");
        sb.Append(stackTrace);
        CreateLogFile();
        FileHelp.WriteFileText(LogFileDir+LogFileName, sb.ToString());
        sb.Clear();
    }

    private static void CreateLogFile()
    {
        if (!Directory.Exists(LogFileDir))
        {
            Directory.CreateDirectory(LogFileDir);
        }

        if (ifClean)
        {
            //每次启动Unity时，删除上一次日志
            if (File.Exists(LogFileDir+ LogFileName))
                File.Delete(LogFileDir + LogFileName);
            return;
        }


        DateTime now = DateTime.Now;
        LogFileName = now.GetDateTimeFormats('s')[0].ToString();//2023-1-05T14:06:25
        LogFileName = LogFileName.Replace("-", "_");
        LogFileName = LogFileName.Replace(":", "_");
        LogFileName = LogFileName.Replace(" ", "");
        LogFileName += ".log";
    }

    private static void LogToFile(string message, bool EnableStack = false)
    {

        if (LogFileWriter == null)
        {

            string fullpath = LogFileDir + LogFileName;
            try
            {
                if (!Directory.Exists(LogFileDir))
                {
                    Directory.CreateDirectory(LogFileDir);
                }

                LogFileWriter = File.AppendText(fullpath);
                LogFileWriter.AutoFlush = true;
            }
            catch (Exception e)
            {
                LogFileWriter = null;
                Debug.LogError("LogToCache() " + e.Message + e.StackTrace);
                return;
            }
        }

        if (LogFileWriter != null)
        {
            try
            {
                LogFileWriter.WriteLine(message);
                if (EnableStack)
                {
                    LogFileWriter.WriteLine(StackTraceUtility.ExtractStackTrace());
                }
            }
            catch (Exception)
            {
                return;
            }
        }
    }



    private static void DebugTtyCatchThreaded(string condition, string stackTrace, LogType type)
    {

    }


    [STAThread]
    public static void Init()
    {
        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Debug.LogErrorFormat("UnhandledException ==========>", (Exception)e.ExceptionObject);
    }
}
