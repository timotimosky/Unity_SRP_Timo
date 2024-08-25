using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ConfigInterface
{
    string GetStringId();

    string ConfigPath();

    IEnumerator AsyncDeserializeObject();

    void SyncDeserializeObject();

    void PushData(byte[] bits);
}

public abstract class Config<T> where T : ConfigInterface,new()
{
    private static T _instance;

    protected static List<T> _contentList = new List<T>();

#if UNITY_EDITOR

    private static string LoadConfigName = string.Empty;

    public static string ConfigName = "";

    public static void SetConfigName(string val)
    {
        ConfigName = val;
        //UnityEngine.Debug.LogError(ConfigName);
    }

#endif

//    private static JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
//    {
//        Error = JsonError
//    };

//    private static void JsonError(object sender, ErrorEventArgs errorEventArgs)
//    {
//#if UNITY_EDITOR
//        Debug.LogErrorFormat("表 {0} json序列化异常 {1}", LoadConfigName, errorEventArgs.ErrorContext.Error.Message);
//#endif
//    }


    public virtual List<T> contentList
    { 
        get
        {
          //  if (!_byteDatas.IsNullOrEmpty())
            {
                SyncDeserializeObject();
            }
            return _contentList;
        }
    }

    public abstract string GetStringId();

    public abstract string ConfigPath();


    public static T Instance()
    {
        if (_instance == null)
            _instance = new T();
        return _instance;
    }


    public T Find(int key)
    {
#if UNITY_EDITOR
        if (ParseConfig.useDic.Contains(ConfigName))
        {
            ParseConfig.useDic.Remove(ConfigName);
        }
#endif
        return Find(key.ToString());
    }


    public T Find(Enum key)
    {
#if UNITY_EDITOR
        if (ParseConfig.useDic.Contains(ConfigName))
        {
            ParseConfig.useDic.Remove(ConfigName);
        }
#endif
        return Find(key.ToString());
    }


    public virtual T Find(string id)
    {
#if UNITY_EDITOR
        if (ParseConfig.useDic.Contains(ConfigName))
        {
            ParseConfig.useDic.Remove(ConfigName);
        }
#endif

       // if (contentList.IsNullOrEmpty())
        {
            Debug.LogFormat("表格{0}数据不存在", ConfigPath());
            return default(T);
        }
        //int count = _contentList.Count;
        //for (int i = 0; i < count; i++)
        //{
        //    T mContent = _contentList[i];

        //    if (mContent.GetStringId().Equals(id))
        //        return mContent;
        //}
#if UNITY_EDITOR
        //Debug.LogErrorFormat("表格{0}数据不存在id为{1}的数据", ConfigPath(), id);
#endif
        //return default(T);
    }

    #region Data
    
    protected List<byte[]> _byteDatas;

    public virtual void PushData(byte[] bytes)
    {
        if (_byteDatas == null)
        {
            _byteDatas = new List<byte[]>();
        }

        _byteDatas.Add(bytes);
    }

    public bool CanAdd(string path)
    {
        return path.IndexOf(ConfigPath(), StringComparison.Ordinal) == 0;
    }


    public void SyncDeserializeObject()
    {
        if (_byteDatas == null)
        {
            return;
        }

#if UNITY_EDITOR
        LoadConfigName = ConfigPath();
        Debug.Log(string.Format("同步加载配置 {0} frame count {1}", ConfigPath(), Time.frameCount));
#endif

        for (int i = 0; i < _byteDatas.Count; i++)
        {
            var element = _byteDatas[i];
            var desData = JsonUtility.FromJson<List<T>>(
                    System.Text.Encoding.UTF8.GetString(element));
            _contentList.AddRange(desData);
        }
        _byteDatas.Clear();
    }


    public IEnumerator AsyncDeserializeObject()
    {
        if (_byteDatas == null)
        {
            yield break;
        }

#if UNITY_EDITOR
        LoadConfigName = ConfigPath();
#endif

        for (int i = 0; i < _byteDatas.Count; i++)
        {
#if UNITY_EDITOR
            StartWatch();
#endif
            var element = _byteDatas[i];
           // if (!element.IsNullOrEmpty())
            {
                var desData = JsonUtility.FromJson<List<T>>(
                    System.Text.Encoding.UTF8.GetString(element));
                _contentList.AddRange(desData);
            }
#if UNITY_EDITOR
            EndWatch(string.Format("异步加载 {0} Index {1}/{2} frame count {3}", ConfigPath(), i + 1, _byteDatas.Count, UnityEngine.Time.frameCount));
#endif
            yield return null;
        }

        _byteDatas.Clear();
    }

#if UNITY_EDITOR

    System.Diagnostics.Stopwatch _stopWatch = new System.Diagnostics.Stopwatch();
    void StartWatch()
    {
        _stopWatch.Reset();
        _stopWatch.Start();
    }


    void EndWatch(string info)
    {
        _stopWatch.Stop();
       // UnityEngine.Debug.LogFormat("{0} : {1}ms", info, _stopWatch.ElapsedMilliseconds);
    }

#endif

    #endregion

}
