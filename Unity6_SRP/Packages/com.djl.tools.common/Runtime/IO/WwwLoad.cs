using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;
/********************************
* Author：	    djl
* Date：       2016/05/02
* Version：	V 0.1.0	
* 
*******************************/
public class WebRequestCertificate : CertificateHandler
{
    //可以通过设置WebRequest.certificateHandler来指定证书，CertificateHandler类的ValidateCertificate函数返回值是false。
    //自定义子类继承CertificateHandler，重写ValidateCertificate函数，就可以成功验证证书了
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        //return base.ValidateCertificate(certificateData);
        return true;
    }

}
public class WwwLoad :MonoBehaviour
{
    public  static WwwLoad Instance;

    private void Awake()
    {
        Instance = this;
    }

    Dictionary<string, int> failedDic = new Dictionary<string, int>();
    const byte FailedMax = 2;

    //如果要自己处理成败，则设置Bool为true
    public void Load(string fileName, Action<UnityWebRequest> CallBack = null, bool CallBackDo = false)
    {
        //DebugTool.LogError("加载文件路径：{0}", fileName);
        StartCoroutine(GetRequest(fileName, CallBack, CallBackDo));
    }

    //private IEnumerator DownLoad(string fileName, Action<WWWCoroutine> callBack, bool callBackDo)
    //{
    //    throw new NotImplementedException();
    //}

    //Get访问
    private IEnumerator GetRequest(string url, Action<UnityWebRequest> finishFun = null, bool CallBackDo = false)
    {
        url = url.Trim().Replace(@"\", "/");
        //string url = "https://www.baidu.com/";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            if (url.Contains("https"))
                webRequest.certificateHandler = new WebRequestCertificate();

            UnityWebRequestAsyncOperation webRequestAsyncOp = webRequest.SendWebRequest();
           // hrs[webRequestAsyncOp] = hr;
            yield return webRequestAsyncOp;

            if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("失败......null.............." + url);
                //DebugTool.LogJL("失败...................." + url + "========" + webRequest.error);
                Debug.LogError(webRequest.error + "\n" + webRequest.downloadHandler.data);
                // ErrorAlert.SystemAlert(string.Format("网络波动,更新失败,请点击重试(1)", url, "www is Null"), (obj) => { Load(url, finishFun); RecordFailed(url); }, new string[] { "点击重试" });
            }
            else if (!webRequest.isDone)
            {
                //loadFaild = true;
                Debug.Log(webRequest.downloadHandler.data);
                if (finishFun != null)
                    finishFun(webRequest);
                // ErrorAlert.SystemAlert("服务器响应超时，请稍候再试", SocketStarter.Instance.QuitGame, new string[] { "退出游戏" });
            }
            else
            {
                Debug.Log("加载成功................." + url);
                if (finishFun != null)
                    finishFun(webRequest);
            }
        }
    }

    private void RecordFailed(string url)
    {
        if (failedDic == null)
            failedDic = new Dictionary<string, int>();
        int result;
        if (failedDic.TryGetValue(url, out result))
        {
            if (result >= FailedMax)
            {
                //ErrorAlert.SystemAlert(string.Format("由于网络波动,更新失败,请点击重试", url), SocketStarter.Instance.QuitGame, new string[] { "退出重试" });
                return;
            }
            failedDic[url]++;
        }
        else
            failedDic[url] = 1;
    }

}
