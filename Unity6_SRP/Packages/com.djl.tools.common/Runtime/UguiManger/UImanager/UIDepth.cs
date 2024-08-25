using UnityEngine;
using UnityEngine.UI;

public class UIDepth : MonoBehaviour
{
    [SerializeField] [HideInInspector] private int _order;
    //是不是窗口，如果挂在特效上，_isView==false
    [SerializeField] [HideInInspector] private bool _isView;
    [SerializeField] [HideInInspector] private bool _autoFindParentCanvas;
    [SerializeField] public Canvas DependCanvas;


    private Canvas _canvas;

    /// <summary>
    /// 这个由UI管理统一调度
    /// </summary>

    public int OffsetOrder;
    public void SetOffsetOrder(int vOffsetOrder)
    {
        OffsetOrder = vOffsetOrder;
    }
    public int GetOffsetOrder()
    {
        return OffsetOrder;
    }


    /// <summary>
    /// 自动查找父层级
    /// </summary>
    public bool AutoFindParent
    {
        get { return _autoFindParentCanvas;}
        set { _autoFindParentCanvas = true; }
    }

    /// <summary>
    /// 设定序列
    /// </summary>
    public int Order
    {
        get { return _order; }
        set
        {
            _order = value;
            Refresh();
        }
    }

    /// <summary>
    /// 是否是UI
    /// </summary>
    public bool IsView
    {
        get { return _isView; }
        set
        {
            _isView = value;
            InitCanvas();
        }
    }


    /// <summary>
    /// 标记为改变,Update时会 Refresh.
    /// </summary>

    public bool IsChanged { get; set; }

    /// <summary>
    /// 改变时通知子节点
    /// </summary>

    public bool NoticeChild { get; set; }

#if UNITY_EDITOR

    void OnEnable()
    {
        Refresh();
    }

#endif

    void Awake()
    {
        InitCanvas();
    }

    void Start()
    {
        Refresh();
    }


    public void Refresh()
    {
        //Debug.LogFormat("SortOrder Depth " , Time.frameCount , " Name " , transform.name , " Depend " , (DependCanvas == null ? "null" : DependCanvas.name));

        if (_autoFindParentCanvas)
        {
            AutoFindParentCanvas();
        }

        if (_isView)
        {
            if (_canvas == null)
            {
                _canvas = GetComponent<Canvas>();
            }

            _canvas.overrideSorting = true;
            _canvas.sortingOrder = SortOrderValue();

            if (NoticeChild)
            {
                TriggerChildsChange();
            }
        }
        else
        {
            Renderer[] renders = this.GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < renders.Length; i++)
            {
                var renderer = renders[i];
                renderer.sortingOrder = SortOrderValue();
            }

            if (NoticeChild)
            {
                TriggerChildsChange();
            }
        }
    }


    /// <summary>
    /// 当前序列值
    /// </summary>
    /// <returns></returns>

    public int SortOrderValue()
    {
        if (DependCanvas != null)
        {
            var depth = DependCanvas.GetComponent<UIDepth>();
            if (depth != null && depth.DependCanvas != this)
                return _order + depth.SortOrderValue();
        }

        return _order + OffsetOrder;
    }


    /// <summary>
    /// 自动查找父画布
    /// </summary>

    public void AutoFindParentCanvas()
    {
        var canvas = GetComponentsInParent<Canvas>(true);

        //  过滤自身
        for (int i = 0; i < canvas.Length; i++)
        {
            var canva = canvas[i];
            if (canva.transform != transform)
            {
                DependCanvas = canva;
                break;
            }
        }
    }


    public void TriggerChildsChange()
    {
        UIDepth[] depths = GetComponentsInChildren<UIDepth>(true);
        for (int i = 0; i < depths.Length; i++)
        {
            var d = depths[i];
            if (d != this)
                d.IsChanged = true;
        }
    }


    private void InitCanvas()
    {
        if (_isView)
        {
            if (_canvas == null)
            {
                _canvas = this.GetComponent<Canvas>();
            }

            if (_canvas == null)
            {
                _canvas = gameObject.AddComponent<Canvas>();
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }
    }

    void Update()
    {
        if (IsChanged)
        {
            Refresh();
            IsChanged = false;
        }
    }


    /// <summary>
    /// 在一个对象上增加一个UIDepth
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="isView"></param>
    /// <param name="order"></param>
    /// <param name="autoFindDepend"></param>
    public static void AddDepth(GameObject obj, bool isView, int order, bool autoFindDepend = true)
    {
        if (obj != null)
        {
            //  必须是UI
			if (obj.GetComponent<RectTransform>() == null)
                return;

            var depth = obj.GetComponent<UIDepth>();

            if (depth == null)
            {
                depth = obj.AddComponent<UIDepth>();
                depth.IsView = isView;
                depth.InitCanvas();
                depth.Order = order;
                depth.IsChanged = true;
                depth.AutoFindParent = autoFindDepend;
            }
        }
    }

}