#define EnableLog
using System;
using System.Text;
using UnityEngine;
// mcs -r:/Applications/Unity/Unity.app/Contents/Frameworks/Managed/UnityEngine.dll -target:library ClassesForDLL.cs
//C:\Program Files\Unity\Editor\Data\Managed

public class DebugTool
{
    static public bool EnableLog = true;
    public static bool EnableSave = true;
    public static bool m_hasForceMono = false;
    public const string WriteKey = "DebugLog";
    public const string DontWriteKey = "noWrite";
    public const string WriteKeyString = "["+ WriteKey + "]";
    [System.Diagnostics.Conditional("EnableLog")]
    static public void LogErrorFormatAndWrite(string format, params object[] args)
    {
#if EnableLog
        StringBuilder sb = new StringBuilder();
        sb.Append("<color=lightblue>");
        sb.Append(WriteKeyString);
        sb.Append(format);
        sb.Append("</color>");
        foreach (object margs in args)
            sb.Append(margs);
        Debug.LogError(sb);

#endif
    }

    static public void Log(params object[] args)
    {
#if EnableLog
        string logRes = StringBuilderCache.Append(WriteKeyString, args);
        Debug.Log(logRes);
#endif
    }
    static public void LogError(params object[] args)
    {
#if EnableLog
       string logRes= StringBuilderCache.Append(WriteKeyString, args);
        Debug.LogError(logRes);
#endif
    }


    static public void Log_lightblue(object message)
    {
        if (EnableLog)
        {
            Debug.LogFormat(WriteKeyString, "<color=lightblue>{0} </color> ", message);
        }
    }

    [System.Diagnostics.Conditional("EnableLog")]
    static public void LogFormat(object format, params object[] args)
    {
      //  Debug.LogFormat(LocateString, "<color=cyan>{0}</color>", message);
#if EnableLog
        StringBuilder sb = new StringBuilder();
            sb.Append("<color=lightblue>"); 
        sb.Append(format);
            sb.Append("</color>");
        sb.Append(WriteKeyString);
        foreach (object margs in args)
                sb.Append(margs);
            Debug.Log(sb);
        
#endif
    }

    static public void Log_lime(object message)
    {
        if (EnableLog)
        {
              Debug.LogWarningFormat(WriteKeyString, "<color=lime>{0}</ color > ", message);
        }
    }

    static public void LogWarning(params object[] message)
    {
        if (EnableLog)
        {
            Debug.LogWarningFormat(WriteKeyString, message);
        }
    }

    static public void LogWarningFormat(string format, params object[] args)
    {
        if (EnableLog)
        {
            Debug.LogWarningFormat(WriteKeyString, format, args);
        }
    }
    static public void LogErrorWithTime(object message)
    {
        if (EnableLog)
        {
            Debug.LogErrorFormat(WriteKeyString, string.Format("<color=#6666ccff>Battle[{0}]</color><color=#FF9224FF>{1}</color>", DateTime.Now.ToString("HH:mm:ss"), message));
        }
    }

    private static string GetLogText(string tag, string message)
    {
        string str = "";
        //if (Debuger.EnableTime)
        {
            DateTime now = DateTime.Now;
            str = now.ToString("HH:mm:ss.fff") + " ";
        }

        str = str + tag + "::" + message;
        return str;
    }

}



