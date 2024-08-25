using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AlertWindow : UguiBase
{

    private ErrorAlert.AlertData _data;
    [SerializeField]
    private Text _alertTxt;
    [SerializeField]
    Text _priceTxt;

    [SerializeField]
    private Image[] _bg;

    [SerializeField]
    private Button _sureBtn;

    [SerializeField]
    private Button _cancelBtn;

    [SerializeField]
    private Button _closeBtn;

    [SerializeField]
    private Toggle _toggle;

    [SerializeField]
    private RectTransform _contents;
    public override void ExecuteParmetersBeforeOpen(params object[] args)
    {
        ErrorAlert.AlertData data = args[0] as ErrorAlert.AlertData;
        if (data != null)
            Init(data);
    }
    public void Init(ErrorAlert.AlertData data)
    {
        _data = data;
        if (string.IsNullOrEmpty(_data.alertStr) == false)
            _alertTxt.text = _data.alertStr;
        string[] arrs = data.param as string[];
        //if (!arrs.IsNullOrEmpty())
        //{
        //    if(arrs.Length > 0 && _sureBtn != null)
        //        _sureBtn.GetComponentInChildren<Text>().text = arrs[0].ToString();
        //    if(arrs.Length > 1 && _cancelBtn != null)
        //        _cancelBtn.GetComponentInChildren<Text>().text = arrs[1].ToString();
        //}

        switch (data.type)
        {
            case AlertType.AlertTypeA:
            case AlertType.AlertTypeF:
            case AlertType.AlertTypeH:
                if (_sureBtn == null || _cancelBtn == null || _closeBtn == null)
                    Debug.LogFormat("WindowUIError");
                _sureBtn.onClick.AddListener(Sure);
                _cancelBtn.onClick.AddListener(Cancel);
                _closeBtn.onClick.AddListener(Close);
                _cancelBtn.gameObject.SetActive(true);
                break;
            case AlertType.AlertTypeC:
            case AlertType.AlertTypeE:
            
                if (_sureBtn == null || _cancelBtn == null || _closeBtn == null)
                    Debug.LogFormat("WindowUIError");
                _sureBtn.onClick.AddListener(Sure);
                _cancelBtn.onClick.AddListener(Cancel);
                _closeBtn.onClick.AddListener(Close);
                _sureBtn.transform.localPosition = new Vector3(0,-234,0);
                _cancelBtn.gameObject.SetActive(false);
                break;
            case AlertType.AlertTypeB:
                if (_sureBtn == null || _cancelBtn == null || _closeBtn == null)
                    Debug.LogFormat("WindowUIError");
                _sureBtn.onClick.AddListener(Sure);
                _cancelBtn.onClick.AddListener(Cancel);
                _closeBtn.onClick.AddListener(Close);
                _toggle.onValueChanged.AddListener(OnSelecteChange);
                break;
            case AlertType.AlertTypeD:
                Invoke("CloseMe", 1.33f);
                //if(!_bg.IsNullOrEmpty())
                //{
                //    //Font font = _alertTxt.font;
                //    int strCount = _alertTxt.text.Length;
                //    float width = (_alertTxt.resizeTextMaxSize - 6) * strCount;
                //    if (width > 1080)
                //        width = 1080;
                //    if(width < 354)
                //        width = 354;
                //    _bg[0].GetComponent<RectTransform>().sizeDelta = new Vector2(width, 104);
                //}
                break;
            case AlertType.AlertTypeG:
                if (_sureBtn == null || _cancelBtn == null || _closeBtn == null)
                    Debug.LogFormat("WindowUIError");
                _sureBtn.onClick.AddListener(Sure);
                _cancelBtn.onClick.AddListener(Cancel);
                _closeBtn.onClick.AddListener(Close);
                _priceTxt.text = _data.priceStr;
                break;
        }
    }


    private void Sure()
    {
        if (_data != null && _data.sureHandler != null)
            _data.sureHandler.Invoke(_data.param != null && _data.param.Length > 0 ? _data.param[0] : null);
        CloseMe();
    }

    private void Cancel()
    {
        if (_data != null && _data.cancelHandler != null)
            _data.cancelHandler.Invoke();
        CloseMe();
    }

    private void Close()
    {
        if (_data != null && _data.closeHandler != null)
            _data.closeHandler.Invoke();
        CloseMe();
    }

    private void OnSelecteChange(bool isOn)
    {
        if (_data != null && _data.selectChange != null)
            _data.selectChange.Invoke(isOn);
    }

    private void TweenOverDestroy()
    {
        ErrorAlert.GetInstance().CloseAlertWindow(this);
        _data = null;
      //  UguiConfig.Instance.CloseWindow(this);
    }

    private void CloseMe()
    {
        base.CloseWindow();
        ErrorAlert.GetInstance().CloseAlertWindow(this);
        _data = null;
      //  UguiConfig.Instance.CloseWindow(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        //ErrorAlert.GetInstance().CloseAlertWindow(this);
        _data = null;
    }
    
}
