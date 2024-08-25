using UnityEngine;

public class NotiConst
{
    /// <summary>
    /// View层消息通知
    /// </summary>
    public const string UPDATE_MESSAGE = "UpdateMessage";           //更新消息
    public const string UPDATE_EXTRACT = "UpdateExtract";           //更新解包
    public const string UPDATE_DOWNLOAD = "UpdateDownload";         //更新下载
    public const string UPDATE_PROGRESS = "UpdateProgress";         //更新进度
}

//用于做loading显示
public class loadPack
{
    public int nowNum;//当前下载数量
    public int AllNum;//总下载数量
    public string name;//资源名字

    public loadPack(int nowNum, int AllNum, string name)
    {
        this.nowNum = nowNum;
        this.AllNum = AllNum;
        this.name = name;
    }
}


public class AppView : MonoBehaviour
{
    public static int MaxNum;

    public static int NowNum;

    public static string State;

    public static AppView Instance;

   // private string message;


    public System.Action<loadPack> LoadBack;

    void Awake() {
        Instance = this;
    }

    //TODO:以后需要跨线程可以再增加缓冲

    //新的处理  用来显示loading条
    public void ExcuteViewMsg(string notice, string msg)
    {
        if (NowNum >= MaxNum)
        {
            Debug.Log("当前阶段下载完毕");
            //DownUpdateManager.Instance.mDownState = DownState.Finish;
        }

        //message = msg;
        NowNum++;
        //TODO：此处增设回调
        if (LoadBack != null)
            LoadBack(new loadPack(NowNum, MaxNum,msg));

    }
}
