using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class UICommonTip : UguiBase {
    public Text m_TextTitle;
    public Text m_TextContent;
    public Button[] m_ButtonList;
    public delegate void DelegateOnButtonClick(TipButtonType vTipButtonType);
    DelegateOnButtonClick _onButtonClick;
    public void OnButtonClick(int vTipButtonType)
    {
        CloseWindow();
        if (_onButtonClick != null)
            _onButtonClick((TipButtonType)vTipButtonType);
    }

    public override void ExecuteParmetersBeforeOpen(params object[] args)
    {
        base.ExecuteParmetersBeforeOpen(args);
        m_TextTitle.text = args[0] as string;
        m_TextContent.text = args[1] as string;
        _onButtonClick = args[2] as DelegateOnButtonClick;

       // m_ButtonList[0]. = args[3] as string;


        if (args.Length > 4 && args[4] != null)
        {
         //   m_ButtonList[1].text = args[4] as string;
            m_ButtonList[1].gameObject.SetActive(true);
        }
        else
        {
            m_ButtonList[1].gameObject.SetActive(false);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _onButtonClick = null;
        //显示下一个
        ShowNext();
    }

    public enum TipButtonType
    {
        OK,
        Cancel
    }

    public class TipInfo{
        public string m_Title;
        public string m_Content;
        public DelegateOnButtonClick m_CallBack;
        public string m_OkText;
        public string m_CancelText;
    }

    static Queue<TipInfo> _queueTipInfo;
    //队列处理
    public static void EnqueueShow(string vTitle,
        string vContent,
         DelegateOnButtonClick vCallBack,
        string vOkText,
        string vCancelText 
       )
    {
        if (_queueTipInfo == null)
            _queueTipInfo = new Queue<TipInfo>();

        _queueTipInfo.Enqueue(new TipInfo() {
            m_Title = vTitle,
            m_Content = vContent,
            m_OkText = vOkText,
            m_CancelText = vCancelText,
            m_CallBack = vCallBack });

        //立刻显示
        if (_queueTipInfo.Count == 1)
        {
            ShowNext();
        }
    }

    static void ShowNext()
    {
        //显示
        if (_queueTipInfo.Count > 0)
        {
            TipInfo info = _queueTipInfo.Dequeue();
            ShowTip(info);
        }
    }

    static void ShowTip(TipInfo vTipInfo)
    {
        //UguiConfig.Instance.ShowWindow("CommonTip", 
        //    vTipInfo.m_Title,
        //    vTipInfo.m_Content,
        //    vTipInfo.m_CallBack,
        //    vTipInfo.m_OkText,
        //    vTipInfo.m_CancelText);
    }
}

