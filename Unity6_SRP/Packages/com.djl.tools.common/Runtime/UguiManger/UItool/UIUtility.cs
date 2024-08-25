using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// **** 创建人    : wcx   		   ******	
// **** 创建时间  : #20170309#
// **** 描述  : UI相关工具类

public static class UIUtility
{



    /// <summary>
    /// 设置RectTransform对象为全屏拉伸
    /// </summary>
    /// <param name="rectTransform"></param>
    public static void SetStretchFullScreen(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();

        if (rt != null)
        {
            SetStretchFullScreen(rt);
        }
    }


    /// <summary>
    /// 设置RectTransform对象为全屏拉伸
    /// </summary>
    /// <param name="rectTransform"></param>
    public static void SetStretchFullScreen(RectTransform rectTransform)
    {
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
    }


    /// <summary>
    /// 设置RectTransform对象为锚点左边
    /// </summary>
    /// <param name="rectTransform"></param>
    public static void SetAnchorLeft(RectTransform rectTransform)
    {
        rectTransform.anchorMax = new Vector2(0, 0.5f);
        rectTransform.anchorMin = new Vector2(0, 0.5f);
    }

    /// <summary>
    /// 设置RectTransform对象为锚点居中
    /// </summary>
    /// <param name="rectTransform"></param>
    public static void SetAnchorCenter(RectTransform rectTransform)
    {
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
    }

    /// <summary>
    /// 设置UguiBase对象拉伸全屏
    /// </summary>
    /// <param name="window"></param>
    public static void SetStretchFullScreen(UguiBase window)
    {
        RectTransform rectTransform = window.CacheRectTransform;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// 设置父节点
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public static GameObject AddChild(GameObject parent, GameObject prefab)
    {
        GameObject go = GameObject.Instantiate(prefab) as GameObject;
#if UNITY_EDITOR
        UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Object");
#endif
        if (go != null && parent != null)
        {
            Transform t = go.transform;
            t.SetParent(parent.transform);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            go.layer = parent.layer;
        }
        return go;
    }


    /// <summary>
    /// 设置父节点
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="child"></param>
    public static void SetParent(GameObject parent, GameObject child)
    {
        if (child != null && parent != null)
        {
            Transform t = child.transform;
            t.SetParent(parent.transform);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            child.layer = parent.layer;
        }
    }


    /// <summary>
    /// 播放统一窗口弹出动画
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="onCompleter"></param>
    public static void PlayScalePopAnim(GameObject obj, System.Action onCompleter)
    {
        //var animator = obj.GetComponent<Animator>();
        //if (animator != null)
        //{
        //    obj.transform.localScale = new Vector3(0.4f, 0.4f, 1);

        //    CheckScalePopAnimEvent(animator, onCompleter);

        //    animator.enabled = true;
        //    animator.Play("tongyong_UItanchu", 0, 0);
        //    var uguiEvent = obj.GetOrAddComponent<UguiAnimEvent>();
        //    uguiEvent.SetCompleter(onCompleter);
        //}
        //else
        //{
        //    obj.transform.localScale = new Vector3(0.4f, 0.4f, 1);

        //    var uguiEvent = obj.GetOrAddComponent<UguiAnimEvent>();
        //    uguiEvent.SetCompleter(onCompleter);

        //    var animCtl = Object.Instantiate(LoadCache.LoadPrefeb<RuntimeAnimatorController>("AnimatorController/tongyong_UItanchu"));
        //    animator = obj.GetOrAddComponent<Animator>();
        //    animator.runtimeAnimatorController = animCtl;

        //    CheckScalePopAnimEvent(animator, onCompleter);
        //}
    }
    /// <summary>
    /// 透明度动画 从透明到不透明一直循环
    /// </summary>
    /// <param name="mg"></param>
    public static void AlphaTween(MaskableGraphic mg)
    {
       // mg.DOFade(0.3f, 0.7f).SetLoops(-1, LoopType.Yoyo);
    }


    /// <summary>
    /// 检查是否有事件
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="onCompleter"></param>
    private static void CheckScalePopAnimEvent(Animator animator, System.Action onCompleter)
    {
        //var clips = animator.runtimeAnimatorController.animationClips;
        //if (clips.IsNullOrEmpty())
        //{
        //    if (onCompleter != null)
        //        onCompleter();
        //}
        //else
        //{
        //    if (clips[0].events.IsNullOrEmpty())
        //    {
        //        AnimationEvent e = new AnimationEvent
        //        {
        //            functionName = "OnAnimPopupFinish",
        //            time = clips[0].length,
        //            messageOptions = SendMessageOptions.DontRequireReceiver
        //        };
        //        clips[0].events = null;
        //        clips[0].AddEvent(e);
        //    }
        //}
    }


    /// <summary>
    /// 添加UIDepth,设置深度值
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="depth"></param>
    public static void SetDepth(GameObject obj, int depth)
    {
        if (obj == null)
        {
            return;
        }

        //var uiDepth = obj.GetOrAddComponent<UIDepth>();
        //uiDepth.AutoFindParent = true;
        //uiDepth.Order = depth;
        //uiDepth.IsChanged = true;
    }


    public static void SetViewDepth(GameObject obj, int depth, Canvas dependCanvas)
    {
        if (obj == null)
        {
            return;
        }

        //var uiDepth = obj.GetOrAddComponent<UIDepth>();
        //uiDepth.AutoFindParent = true;
        //uiDepth.IsView = true;
        //uiDepth.DependCanvas = dependCanvas;
        //uiDepth.Order = depth;
        //uiDepth.IsChanged = true;
    }

    /// <summary>
    /// Recursively set the game object's layer.
    /// </summary>
    public static void SetLayer(GameObject obj, int layer)
    {
        obj.layer = layer;

        Transform t = obj.transform;

        for (int i = 0, imax = t.childCount; i < imax; ++i)
        {
            Transform child = t.GetChild(i);
            SetLayer(child.gameObject, layer);
        }
    }

    /// <summary>
    /// Returns the hierarchy of the object in a human-readable format.
    /// </summary>

    public static string GetHierarchy(GameObject obj)
    {
        string path = obj.name;

        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }
        return "\"" + path + "\"";
    }

    #region 指定一个 item让其定位到ScrollRect中间

    /// <summary>
    /// 指定一个 item让其定位到ScrollRect中间
    /// </summary>
    /// <param name="target">需要定位到的目标</param>
    public static void CenterOnItem(RectTransform target, ScrollRect scrollRect, RectTransform viewPointTransform, RectTransform contentTransform)
    {

        var itemCenterPositionInScroll = GetWorldPointInWidget(scrollRect.GetComponent<RectTransform>(), GetWidgetWorldPoint(target));

        var targetPositionInScroll = GetWorldPointInWidget(scrollRect.GetComponent<RectTransform>(), GetWidgetWorldPoint(viewPointTransform));

        var difference = targetPositionInScroll - itemCenterPositionInScroll;

        difference.z = 0f;

        var newNormalizedPosition = new Vector2(difference.x / (contentTransform.rect.width - viewPointTransform.rect.width),
            difference.y / (contentTransform.rect.height - viewPointTransform.rect.height));

        newNormalizedPosition = scrollRect.normalizedPosition - newNormalizedPosition;

        newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x);
        newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y);

        scrollRect.normalizedPosition = newNormalizedPosition;

        //DOTween.To(() => scrollRect.normalizedPosition, x => scrollRect.normalizedPosition = x, newNormalizedPosition, 3);
    }

    static Vector3 GetWidgetWorldPoint(RectTransform target)
    {
        //pivot position + item size has to be included
        var pivotOffset = new Vector3(
            (0.5f - target.pivot.x) * target.rect.size.x,
            (0.5f - target.pivot.y) * target.rect.size.y,
            0f);
        var localPosition = target.localPosition + pivotOffset;
        return target.parent.TransformPoint(localPosition);
    }

    static Vector3 GetWorldPointInWidget(RectTransform target, Vector3 worldPoint)
    {
        return target.InverseTransformPoint(worldPoint);
    }




    #endregion
}
