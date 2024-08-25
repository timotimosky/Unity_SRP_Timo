using UnityEngine;
using System.Collections.Generic;
using System;

public class ErrorAlert
{
    /// <summary>
    /// 
    /// </summary>
    public delegate void OnSure(params object[] p);

    /// <summary>
    /// 
    /// </summary>
    public delegate void OnCancel(params object[] p);

    /// <summary>
    /// 
    /// </summary>
    public delegate void OnClose(params object[] p);

    /// <summary>
    /// 
    /// </summary>
    public delegate void OnSelectChange(bool isOn, params object[] p);

    /// <summary>
    /// 
    /// </summary>
    private Queue<AlertData> _charactorQueue;

    /// <summary>
    /// 
    /// </summary>
    private Queue<AlertData> _windowQueue;

    /// <summary>
    /// 
    /// </summary>
    private AlertData _currentAlert;

    private static ErrorAlert _instance;

    static ErrorAlert()
    {
        _instance = new ErrorAlert();
    }

    private ErrorAlert()
    {
        _windowQueue = new Queue<AlertData>();
        _charactorQueue = new Queue<AlertData>();
    }

    public static void AlertNetworkError(int code)
    {
        //ErrorListSetting es = ErrorListSetting.Instance().Find(code);
        //if (es != null)
        //{
        //    switch ((AlertType) es.alertType)
        //    {
        //        case AlertType.AlertTypeA:
        //            ///纯文字弹窗，不管
        //            break;
        //        case AlertType.AlertTypeB:
        //            break;
        //        case AlertType.AlertTypeC:
        //            if (es != null)
        //                Alert(es.errorInfo, null, null);
        //            break;
        //        case AlertType.AlertTypeD:
        //            if(es != null)
        //                Alert(es.errorInfo);
        //            break;
        //    }
        //}
        //else
        //{
        //    Alert(string.Format("返回错误码 [{0}], 错误码没有配置", code));
        //}
    }

    public static ErrorAlert GetInstance()
    {
        return _instance;
    }

    public static void SystemAlert(string info, OnSure sureHandler, object[] param)
    {
        if (_instance.CheceWindowRepititionAlert(info))
            return;
        AlertData data = new AlertData();
        data.type = AlertType.AlertTypeE;
        data.sureHandler = sureHandler;
        data.alertStr = info;
        data.param = param;
        _instance.AlertEnQueue(data, true);
    }

    public static void SystemAlert(string info, OnSure sureHandler, OnCancel cancelHandler, object[] param)
    {
        if (_instance.CheceWindowRepititionAlert(info))
            return;
        AlertData data = new AlertData();
        data.type = AlertType.AlertTypeF;
        data.sureHandler = sureHandler;
        data.cancelHandler = cancelHandler;
        data.alertStr = info;
        data.param = param;
        _instance.AlertEnQueue(data, true);
    }

    public static void Alert(string errorInfo)
    {
        AlertData data = new AlertData();
        data.type = AlertType.AlertTypeD;
        data.alertStr = errorInfo;
        _instance.AlertEnQueue(data, false);
    }

    public static void AddAlert(string errorInfo, float intervel, float statetime)
    {
        AlertData data = new AlertData();
        data.inverval = intervel;
        data.statetime = statetime;
        data.type = AlertType.AlertTypeD;
        data.alertStr = errorInfo;
        _instance.AlertEnQueue(data, false);
    }

    public static void Alert(string info, OnSure sureHandler, object[] param)
    {
        if (_instance.CheceWindowRepititionAlert(info))
            return;
        AlertData data = new AlertData();
        data.type = AlertType.AlertTypeC;
        data.sureHandler = sureHandler;
        data.alertStr = info;
        data.param = param;
        _instance.AlertEnQueue(data, true);
    }

    public static void Alert(string info, OnSure sureHandler, OnCancel cancelHandler, object[] param)
    {
        if (_instance.CheceWindowRepititionAlert(info))
            return;
        AlertData data = new AlertData();
        data.type = AlertType.AlertTypeA;
        data.alertStr = info;
        data.sureHandler = sureHandler;
        data.cancelHandler = cancelHandler;
        data.param = param;
        _instance.AlertEnQueue(data, true);
    }

    public static void AlertBack(string info, OnSure sureHandler, OnCancel cancelHandler, object[] param)
    {
        if (_instance.CheceWindowRepititionAlert(info))
            return;
        AlertData data = new AlertData();
        data.type = AlertType.AlertTypeH;
        data.alertStr = info;
        data.sureHandler = sureHandler;
        data.cancelHandler = cancelHandler;
        data.param = param;
        _instance.AlertEnQueue(data, true);
    }

    public static void Alert(string info, string _price, OnSure sureHandler, OnCancel cancelHandler, object[] param)
    {
        if (_instance.CheceWindowRepititionAlert(info))
            return;
        AlertData data = new AlertData();
        data.type = AlertType.AlertTypeG;
        data.alertStr = info;
        data.priceStr = _price;
        data.sureHandler = sureHandler;
        data.cancelHandler = cancelHandler;
        data.param = param;
        _instance.AlertEnQueue(data, true);
    }

    public static void Alert(string info, OnSure sureHandler, OnCancel cancelHandler, OnSelectChange selectChange,
        object[] param)
    {
        if (_instance.CheceWindowRepititionAlert(info))
            return;
        AlertData data = new AlertData();
        data.type = AlertType.AlertTypeB;
        data.alertStr = info;
        data.sureHandler = sureHandler;
        data.cancelHandler = cancelHandler;
        data.selectChange = selectChange;
        data.param = param;
        _instance.AlertEnQueue(data, true);
    }

    private bool CheceWindowRepititionAlert(string info)
    {
        if (_windowQueue != null && _windowQueue.Count > 0)
        {
            var a = _windowQueue.GetEnumerator();
            while (a.MoveNext())
            {
                if (a.Current.alertStr == info)
                    return true;
            }
        }
        return false;
    }

    private void AlertEnQueue(AlertData data, bool bewindow)
    {
        if (_currentAlert != null)
        {
            if (_currentAlert.Equals(data))
            {
                return;
            }
        }
        if (bewindow) _windowQueue.Enqueue(data);
        else AddAlertEnQueueForChar(data);
        
    }

    private void AddAlertEnQueueForChar(AlertData data)
    {
        if (curAlertData.alertStr == data.alertStr)
        {
            return;
        }
        for (int i = 0; i < _charactorQueue.Count; i++)
        {
            AlertData data1 = _charactorQueue.Peek();
            if (data1.alertStr == data.alertStr)
            {
                return;
            }
        }
        _charactorQueue.Enqueue(data);
        ShowElertD();
    }

    /// <summary>
    /// 
    /// </summary>
    public void DoUpdate()
    {
        if (_windowQueue.Count > 0)
        {
            if (_currentAlert != null)
                return;
            AlertData data = _windowQueue.Dequeue();
            AlertType type = data.type;
            string uitype = string.Empty;
            switch (type)
            {
                case AlertType.AlertTypeA:
                    uitype = "ErrorAlertC";
                    break;
                case AlertType.AlertTypeB:
                    uitype = "ErrorAlertB";
                    break;
                case AlertType.AlertTypeC:
                    uitype = "ErrorAlertC";
                    break;
                case AlertType.AlertTypeE:
                    uitype = "ErrorAlertE";
                    break;
                case AlertType.AlertTypeF:
                    uitype = "ErrorAlertF";
                    break;
                case AlertType.AlertTypeG:
                    uitype = "ErrorAlertG";
                    break;
                case AlertType.AlertTypeH:
                    uitype = "ErrorAlertH";
                    break;
            }

            if (uitype != string.Empty)
            {
                //if (UguiConfig.Instance.ShowWindow(uitype, data) != null)
                //{
                //    _currentAlert = data;
                //}
            }
        }
    }

    private bool isAlertD = false;
    private AlertData curAlertData= new AlertData();
    public void ShowElertD()
    {
        if (isAlertD)
        {
            return;
        }
        if (_charactorQueue.Count > 0)
        {
            Debug.LogFormat("ShowElertD");
            isAlertD = true;
            curAlertData = _charactorQueue.Dequeue();
            //UguiConfig.Instance.ShowAlertTips("ErrorAlertD", curAlertData);
        }
    }

    /// <summary>
    /// 关闭弹窗
    /// </summary>
    public void CloseAlertWindow(AlertWindow awindow)
    {
        //if (awindow.Type != "ErrorAlertD")
        //{
        //    _currentAlert = null;
        //}
        //else
        //{
        //    curAlertData = new AlertData();
        //    isAlertD = false;
        //    ShowElertD();
        //}
    }
    /// <summary>
    /// 过场景的时候两个队列都要清空
    /// </summary>
    public void ClearWindowQueue()
    {
        _windowQueue = new Queue<AlertData>();
        _charactorQueue = new Queue<AlertData>();
        _currentAlert = null;
    }

    public class AlertData
    {
        public AlertType type;

        public float inverval = 0.01f;

        public float statetime = 0.5f;

        public string alertStr;

        public string priceStr="";

        public string surBtnTxt;

        public string cancelBtnTxt;

        public Color strColor;

        public OnSure sureHandler;

        public OnCancel cancelHandler;

        public OnClose closeHandler;

        public OnSelectChange selectChange;

        public object[] param;

        public bool Equals(AlertData b)
        {
            return this.type == b.type &&
                   //this.inverval == b.inverval &&
                   //this.inverval == b.inverval &&
                   this.alertStr == b.alertStr;/*&&*/
                   //this.sureHandler == b.sureHandler &&
                   //this.cancelHandler == b.cancelHandler &&
                   //this.selectChange == b.selectChange;
        }
    }
}



/// <summary>
/// 类型
/// </summary>
public enum AlertType
{
    /// <summary>
    /// 确定、取消、关闭按钮
    /// </summary>
    AlertTypeA,
    /// <summary>
    /// 确定、取消、关闭按钮、勾选提示
    /// </summary>
    AlertTypeB,
    /// <summary>
    /// 确定按钮
    /// </summary>
    AlertTypeC,
    /// <summary>
    /// 飘字
    /// </summary>
    AlertTypeD,
    /// <summary>
    ///  只有一个确定按钮 ， 层级在最上层
    /// </summary>
    AlertTypeE,
    /// <summary>
    /// 两个按钮
    /// </summary>
    AlertTypeF,
    /// <summary>
    /// 有钻石图标，专门用于有价格的
    /// </summary>
    AlertTypeG,
    /// <summary>
    /// 取消、确定
    /// </summary>
    AlertTypeH,
}