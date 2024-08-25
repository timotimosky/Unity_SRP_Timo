using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UguiManager
{
    private static bool IsInit;
    //目前缓存的所有UI
    public static Dictionary<string, UguiBase> _cacheUguiBases = new Dictionary<string, UguiBase>();
    //目前已创建的所有层级
    public static Dictionary<UILayer, UguiLayer> mAllLayerDic = new Dictionary<UILayer, UguiLayer>();

    public static void Init()
    {
        if (!IsInit)
        {
            GetLayer(UILayer.BaseLayer);
            GetLayer(UILayer.LowLayer);
            GetLayer(UILayer.Normal);
            GetLayer(UILayer.Middle);
            GetLayer(UILayer.Tips);
            GetLayer(UILayer.Rewards);
            GetLayer(UILayer.TopUILabyer);
            GetLayer(UILayer.Guild);
            GetLayer(UILayer.Wait);
            GetLayer(UILayer.Download);
            GetLayer(UILayer.TopTopTop);
            GetLayer(UILayer.TopTopTopTop);
            IsInit = true;
        }
    }
    
    public static UguiLayer GetLayer(UILayer layer)
    {
        UguiLayer uguiLayer;

        UguiManager.mAllLayerDic.TryGetValue(layer, out uguiLayer);

        if (uguiLayer == null)
        {
            //测试用  待优化
            uguiLayer = CreateLayer(layer, GameObject.Find("UIRoot").transform);
        }

        return uguiLayer;
    }

    public static UguiLayer CreateLayer(UILayer layer, Transform parent)
    {
        GameObject layerObject = new GameObject(layer.ToString());

        layerObject.transform.SetParent(parent);
        layerObject.layer = parent.gameObject.layer;

        RectTransform rectTransform = layerObject.AddComponent<RectTransform>();
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        layerObject.transform.localScale = Vector3.one;
        layerObject.transform.localPosition = Vector3.zero;

        Canvas mCanvas = layerObject.AddComponent<Canvas>();
        mCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        UguiLayer uguiLayer = layerObject.AddComponent<UguiLayer>();
        uguiLayer._startSort = (int)layer * 1000;
        mAllLayerDic.Add(layer, uguiLayer);
        return uguiLayer;
    }

    // Start is called before the first frame update
    public static void CloseUI(UguiBase uguiBase)
    {
        UIBaseInfo des = uguiBase.mUIBaseInfo;
        if (des.mUseType == UseType.Scene || des.mUseType == UseType.ever)
        {
            _cacheUguiBases.Remove(uguiBase.mUIBaseInfo.mResPath);
            UguiLayer uguiLayer = GetLayer(uguiBase.mUIBaseInfo.Layer);
            uguiLayer.Pop(uguiBase);
            Object.Destroy(uguiBase.gameObject);
            return;
        }

        if (!_cacheUguiBases.ContainsValue(uguiBase))
        {
            _cacheUguiBases.Add(uguiBase.mUIBaseInfo.mResPath, uguiBase);
            uguiBase.TriggerDisable();
            uguiBase.CacheRectTransform.localScale = Vector3.zero;
        }
    }

    //UI的单独管理
    public static UguiBase LoadUI(string path, UILayer uILayer = UILayer.BaseLayer, UseType mUseType = UseType.Scene)
    {
        UguiBase window = null;

        if (!_cacheUguiBases.TryGetValue(path, out window))
        {
            //TODO: 与_windowsList销毁冲突可能
            //UguiBase window = GetUICache(type);
            //if (window != null)//新的窗口
            //{
            //    return window;
            //}

            GameObject newWndPrefeb = Resources.Load(path) as GameObject;
            GameObject newWnd = GameObject.Instantiate(newWndPrefeb) as GameObject;
            //ResourceLoadManager.Load<GameObject>(path, mUseType);
            if (newWnd == null)
            {
                Debug.LogError("Instantiate == null!! path:" + path);
                return null;
            }
            window = newWnd.GetComponent<UguiBase>();
            if (window == null)
            {
                Debug.LogError("GetComponent<UguiBase>() == null!! path:" + path);
                return null;
            }

            //暂时放在这里
            UIBaseInfo des = newWnd.GetOrAddComponent<UIBaseInfo>();
            des.mResPath = path;
            des.mUseType = mUseType;
            des.Layer = uILayer;

            window.mUIBaseInfo = des;
            UguiLayer tUguiLayer = GetLayer(uILayer);
            tUguiLayer.Push(window);
            _cacheUguiBases.Add(window.mUIBaseInfo.mResPath, window);
        }
        
        return window;
    }

    public static void UnLoadUI()
    {
        IsInit = false;
        _cacheUguiBases.Clear();
        mAllLayerDic.Clear();
    }
}
