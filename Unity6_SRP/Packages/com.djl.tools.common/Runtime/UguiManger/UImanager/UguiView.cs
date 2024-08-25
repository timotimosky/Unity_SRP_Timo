using UnityEngine;
using UnityEngine.UI;

public class UguiView : MonoBehaviour
{

    private Transform _cacheTransform;
    UIBaseInfo[] componentObjects;

    private GameObject _cacheGameObject;
    private RectTransform _cacheRectTransform;

    public GameObject CacheGameObject
    {
        get { return _cacheGameObject ?? (_cacheGameObject = gameObject); }
    }


    public Transform CacheTransform
    {
        get { return _cacheTransform ?? (_cacheTransform = transform); }
    }


    public RectTransform CacheRectTransform
    {
        get { return _cacheRectTransform ?? (_cacheRectTransform = GetComponent<RectTransform>()); }
    }
    public void MustFirstInit(GameObject obj)
    {
        _cacheGameObject = obj;
        componentObjects = _cacheGameObject.GetComponentsInChildren<UIBaseInfo>(true);
    }

    public GameObject GetObject(string componentName)
    {
        for (int i = 0; i < componentObjects.Length; i++)
        {
            if (componentObjects[i].mResPath == componentName)
                return componentObjects[i].gameObject;
        }

        return null;
    }

    protected virtual void Awake()
    {
    }

    protected virtual void Start() { }

    protected virtual void OnEnable() { }

    protected virtual void OnDisable() { }

    protected virtual void OnDestroy()
    {

    }

    public virtual GameObject GetChild(string obj)
    {
       Transform trans= this.transform.Find(obj);
        if (trans == null)
        {
            Debug.LogErrorFormat("找不到Child路径：{0}" , obj);
            return null;
        }
        return trans.gameObject;
    }

  
    public HorizontalLayoutGroup SwapHLayoutGroup(string name)
    {
        return GetChild(name).GetComponent<HorizontalLayoutGroup>();
    }
    public VerticalLayoutGroup SwapVLayoutGroup(string name)
    {
        return GetChild(name).GetComponent<VerticalLayoutGroup>();
    }

    public Button SwapButton(string name)
    {
        return GetChild(name).GetComponent<Button>();
    }


    public RawImage SwapRawImage(string name)
    {
        return GetChild(name).GetComponent<RawImage>();
    }

    public InputField SwapInputField(string name)
    {
        return GetChild(name).GetComponent<InputField>();
    }

    public Text SwapText(string name)
    {
        var obj = GetChild(name);
        if (obj != null)
            return obj.GetComponent<Text>();
        return null;
    }

    public Slider SwapSlider(string name)
    {
        return GetChild(name).GetComponent<Slider>();
    }

    public ToggleGroup SwapToggleGroup(string name)
    {
        return GetChild(name).GetComponent<ToggleGroup>();
    }


    public Toggle SwapToggle(string name)
    {
        return GetChild(name).GetComponent<Toggle>();
    }

    public Image SwapImage(string name)
    {
        return GetChild(name).GetComponent<Image>();
    }


    #region Component

    public GameObject FindChild(string childName)
    {
        var trans = CacheTransform.Find(childName);
        if (trans == null)
        {
           // Debug.LogJL(string.Format("child is not find, name: {0}", childName));
            return null;
        }

        return trans.gameObject;
    }

    #endregion


    #region Timer


    private string _groupKey;

    public virtual string GroupKey
    {
        get
        {
            if (string.IsNullOrEmpty(_groupKey))
            {
                _groupKey = GetInstanceID().ToString();
            }
            return _groupKey;
        }
    }



    #endregion


}