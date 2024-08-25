/********************************
 * Author		：	cc
 * Date			：	2016/8/9
 * Version		：	V 0.1.0	
 * 
 *******************************/
using System.Collections.Generic;
using UnityEngine;

public class UguiLayer : MonoBehaviour
{
    /// <summary>
    /// 这个层次
    /// </summary>
    public int _startSort;

    public List<UguiBase> _windowsList = new List<UguiBase>();

    /// <summary>
    /// 是否包含该窗口
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>

    public bool Contain(UguiBase window)
    {
        for (int i = 0; i < _windowsList.Count; i++)
        {
            if (_windowsList[i] == window)
                return true;
        }
        return false;
    }

    public List<UguiBase> WindowList
    {
        get
        {
            return _windowsList;
        }
    }

    //添加到窗口列表最后
    public void Push(UguiBase window)
    {
        if (!Contain(window))
        {
            //不存在，则初始化窗口的参数
            IniParent(window);
        }
        else
            _windowsList.Remove(window);

        _windowsList.Add(window);
        window.CacheRectTransform.SetAsLastSibling();

        SortOrder();
    }

    public void Pop(UguiBase window)
    {
        for (int i = 0; i < _windowsList.Count; i++)
        {
            var layer = _windowsList[i];
            if (layer == window)
            {
                _windowsList.RemoveAt(i);
                break;
            }
        }
    }

    //public UguiBase GetWindow(string type)
    //{
    //    for (int i = 0; i < _windowsList.Count; i++)
    //    {
    //        var layer = _windowsList[i];
    //        if (layer.Type == type)
    //            return layer;
    //    }

    //    return null;
    //}

    public UguiBase GetWindowByName(string name)
    {
        for (int i = 0; i < _windowsList.Count; i++)
        {
            var layer = _windowsList[i];
            if (layer.CacheGameObject.name.Contains(name))
                return layer;
        }

        return null;
    }

    public void ClearAll()
    {
        for (int i = _windowsList.Count - 1; i >= 0; i--)
        {
            var window = _windowsList[i];

            if (window != null)
            {
                //UguiDescription setting = UguiSetting.Get(window.Type);
                //if (setting != null && setting.DontDestoryOnJumpScene)
                //    continue;

                //UguiConfig.Instance.DestoryWindow(window);
            }
  
            _windowsList.RemoveAt(i);
        }
        SortOrder();
    }

    public void IniParent( UguiBase window )
    {
        window.CacheTransform.SetParent(transform);
        window.CacheTransform.localPosition = Vector3.zero;
        window.CacheTransform.localScale = Vector3.one;


        RectTransform m_RectTrans = window.CacheRectTransform;
        if (m_RectTrans != null)
        {
            m_RectTrans.anchorMin = Vector2.zero;
            m_RectTrans.anchorMax = Vector2.one;
            m_RectTrans.offsetMin = Vector2.zero;
            m_RectTrans.offsetMax = Vector2.zero;
        }

        UIDepth depth = window.GetComponent<UIDepth>();
        if (depth == null)
        {
            depth = window.gameObject.AddComponent<UIDepth>();
            depth.IsView = true;
        }
        //UguiDescription description = UguiSetting.Get(window.Type);
        //if (description.IsFullUI)
        //    window.SetMask(LoadUtility.EmbedMask(window.CacheTransform));
        //if (description.Ani)
         //   window.ScalePop();
    }

    /// <summary>
    /// 排列
    /// </summary>
    /// <returns></returns>

    internal int SortOrder( )
    {
        int startDepth = _startSort;

        for (int i = 0; i < _windowsList.Count; i++)
        {
            startDepth = _windowsList[i].SetDepth(startDepth);
        }

        return startDepth;
    }


    /// <summary>
    /// 返回最上层窗口
    /// </summary>

    public UguiBase TopWindow
    {
        get
        {
            if (_windowsList.Count == 0)
                return null;
            return _windowsList[_windowsList.Count - 1];
        }
    }

    internal int GetTopOrderValue()
    {
        var top = TopWindow;
        if (top != null)
        {
            return top.LastDepth().GetOffsetOrder();
        }
        return _startSort;
    }


    private void Insert(UguiBase window, int index)
    {
        if (Contain(window))
        {
            _windowsList.Remove(window);
        }

        index = Mathf.Clamp(index, 0, _windowsList.Count);
  
        if (index == _windowsList.Count)
        {
            _windowsList.Add(window);
            window.CacheRectTransform.SetAsLastSibling();
        }
        else if (index == 0)
        {
            _windowsList.Insert(0, window);
            window.CacheRectTransform.SetAsFirstSibling();
        }
        else
        {
            _windowsList.Insert(index, window);
            window.CacheRectTransform.SetSiblingIndex(index);
        }
    }

}
