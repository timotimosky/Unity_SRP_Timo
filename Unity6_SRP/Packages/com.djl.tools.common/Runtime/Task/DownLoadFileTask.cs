using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using UnityEngine;

public class NotiData
{
    public string evName;
    public object evParam;

    public NotiData(string name, object param)
    {
        this.evName = name;
        this.evParam = param;
    }
}


public class DownLoadFileTask : MonoBehaviour
{
    private Stopwatch sw = new Stopwatch();
    string currDownFile;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// �����ļ�
    /// </summary>
    public void OnDownloadFile(List<object> evParams)
    {
        string url = evParams[0].ToString();
        currDownFile = evParams[1].ToString();

        using (WebClient client = new WebClient())
        {
            sw.Start();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            client.DownloadFileAsync(new System.Uri(url), currDownFile);
        }
    }

    private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {

        string value = string.Format("{0} kb/s", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));
        NotiData data = new NotiData(NotiConst.UPDATE_PROGRESS, value);

        OnSyncEvent(data);

        if (e.ProgressPercentage == 100 && e.BytesReceived == e.TotalBytesToReceive)
        {
            sw.Reset();

            data = new NotiData(NotiConst.UPDATE_DOWNLOAD, currDownFile);
            OnSyncEvent(data);
        }
    }

    /// <summary>
    /// ֪ͨ�¼�
    /// </summary>
    /// <param name="state"></param>
    private void OnSyncEvent(NotiData data)
    {
     //   if (this.func != null)
        //    func(data);  //�ص��߼���

        UnityEngine.Debug.LogError("�����߳� ֪ͨView��");
        AppView.Instance.ExcuteViewMsg(data.evName, data.evParam.ToString());//֪ͨView��
    }
}
