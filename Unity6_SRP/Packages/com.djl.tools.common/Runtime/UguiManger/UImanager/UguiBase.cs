using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/********************************
* Author：	    djl
* Date：       2016//
* Version：	V 0.1.0	
* 所有UI继承该类,一个UI预制体绑定一个UguiBase
* 
* 实现类WPF的数据绑定,
* 方法：
* 数据set方法类调用对应的UI更新。注册数据更新，数据更新函数会调用对应UI的updateUI函数。
* 目的：
*       1.防止一个数据变化，多个模块UI需要更新，导致更新遗漏。
*       2.防止一个数据变化，刷新一大批UI，而不是针对刷新单独UI
*       3.实现小红点功能，当某一数据更新后，玩家没有开启过该系统，则提示玩家
*******************************/
[System.Serializable]
public class UguiBase : UguiView
{

    //遮罩
    protected GameObject _mask;

    private Transform _cacheContents;

    public UIBaseInfo mUIBaseInfo;

    /// <summary>
    /// 暂时。。。缓存UI
    /// </summary>
    /// <param name="uguiType"></param>
    /// <returns></returns>
    public static UguiBase GetUICache(string uguiType)
    {
        UguiBase outUguiBase;
        if (UguiManager._cacheUguiBases.TryGetValue(uguiType, out outUguiBase))
        {
            UguiManager._cacheUguiBases.Remove(uguiType);
            outUguiBase.TriggerEnable();
            outUguiBase.CacheRectTransform.localScale = Vector3.one;
            return outUguiBase;
        }
        return null;
    }

    /// <summary>
    /// 下面的子的父节点
    /// </summary>
    public Transform CacheContents
    {
        get { return _cacheContents ?? (_cacheContents = transform.Find("Contents")); }
    }

    /// <summary>
    /// 关闭当前窗口
    /// </summary>

    public virtual void CloseWindow()
    {
        UguiManager.CloseUI(this);
    }


    /// <summary>
    /// 窗口被关闭回调
    /// </summary>

    public virtual void OnCloseWindow()
    {

    }



    /// <summary>
    /// 窗口打开前
    /// </summary>
    /// <param name="args"></param>

    public virtual void ExecuteParmetersBeforeOpen(params object[] args)
    {
        SetStretchFullScreen();
    }


    /// <summary>
    /// 处理消息
    /// </summary>
    /// <param name="args"></param>

    public virtual void WindowMessageHandle(params object[] args)
    {

    }


    /// <summary>
    /// 设置为拉伸全屏
    /// </summary>

    protected void SetStretchFullScreen()
    {
        UIUtility.SetStretchFullScreen(this);
    }


    /// <summary>
    /// 界面push到最上层调用
    /// </summary>
    /// <param name="args"></param>

    public virtual void PushRefresh(params object[] args)
    {

    }


    /// <summary>
    /// 从缓存区激活时调用
    /// </summary>

    internal void TriggerEnable()
    {
        OnEnable();
    }

    internal UIDepth LastDepth()
    {
        var canvasList = DepthContainer;
        if (canvasList.Length > 0)
            return canvasList[canvasList.Length - 1];
        return null;
    }

    /// <summary>
    /// 界面放入缓存区时调用
    /// </summary>

    internal void TriggerDisable()
    {
        OnDisable();
    }


    #region 窗口动画

    public virtual void ScalePop(params object[] args)
    {
        //  TODO 美术给的动画
        if (CacheContents != null)
            UIUtility.PlayScalePopAnim(CacheContents.gameObject, OnAnimationFinished);
    }


    public virtual void OnAnimationFinished()
    {
       // EventObject2 eo = new EventObject2(null, GuideConst.UIOpenAnimFinished);
        //eo.ParamPairs["type"] = this.Type.ToString();
       // GuideManager.Instance.Publish(eo);

       // Debug.LogFormat("Contents " , CacheContents.localScale , " Frame " , Time.frameCount);
    }

    #endregion

    #region 遮罩


    public void SetMask(GameObject vMask)
    {
        _mask = vMask;
    }


    protected Image GetMask()
    {
        return _mask.GetComponentInChildren<Image>(true);
    }

    #endregion

    #region 公共资源栏 界面头顶的 金币 钻石等

   

    #endregion

    #region 增加全屏遮罩

    private GameObject _lockMask;
    [UnityEngine.ContextMenu("LockEvents")]
    public void LockEvents()
    {
        if (_lockMask == null)
        {
            //_lockMask = gameObject.AddChild();
            //var image = _lockMask.AddComponent<Image>();
            //image.color = new Color(1, 1, 1, 0);
            //RectTransform rectTransform = image.transform as RectTransform;

            //UIUtility.SetStretchFullScreen(rectTransform);
            //if (rectTransform != null)
            //    rectTransform.SetAsLastSibling();
        }
        else
        {
            _lockMask.SetActive(true);
        }
    }

    [UnityEngine.ContextMenu("UnLockEvents")]
    public void UnLockEvents()
    {
        if (_lockMask != null)
            _lockMask.SetActive(false);
    }

    #endregion

    #region 深度管理

    private UIDepth[] _depths;
    private ScrollRect[] _scrollRects;

    public void ClearCache()
    {
        _depths = null;
        _scrollRects = null;
    }


    public UIDepth[] DepthContainer
    {
        get
        {
            if (_depths == null)
            {
                _depths = CacheGameObject.GetComponentsInChildren<UIDepth>(true);
                if (_depths == null)
                    _depths = new UIDepth[0];
            }
            return _depths;
        }
    }


    public ScrollRect[] ScrollContainer
    {
        get
        {
            if (_scrollRects == null)
            {
                _scrollRects = CacheGameObject.GetComponentsInChildren<ScrollRect>(true);
                if (_scrollRects == null)
                    _scrollRects = new ScrollRect[0];
            }
            return _scrollRects;
        }
    }


    internal int SetDepth(int startSort)
    {
        UIDepth[] canvasList = DepthContainer;

        for (int i = 0; i < canvasList.Length; i++)
        {
            UIDepth depth = canvasList[i];
            depth.SetOffsetOrder(startSort + i);
            depth.IsChanged = true;
            depth.NoticeChild = true;
        }

        return GetDepthEndValue() + 5;
    }


    public int GetDepthEndValue()
    {
        var depth = GetComponent<UIDepth>();
        if (depth != null)
        {
            return depth.SortOrderValue() + DepthContainer.Length + ScrollContainer.Length;
        }

        //Debug.LogErrorFormat("窗口没有UIDepth wtf? {0}", Type);
        return 0;
    }

    #endregion
}