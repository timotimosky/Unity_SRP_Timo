using UnityEngine;
using System.Collections;

/********************************
 * Author		：	cc
 * Date			：	2016/8/9
 * Version		：	V 0.1.0	
 * 
 *******************************/


public class UguiAnimEvent : MonoBehaviour
{
    private bool _callFinished;
    private System.Action _onCompleter;


    public void SetCompleter(System.Action completer)
    {
        _onCompleter = completer;
    }


    public void OnAnimPopupFinish()
    {
        if (_onCompleter != null)
        {
            _callFinished = true;
        }
        else
        {
            Hide();
        }
    }


    private void LateUpdate()
    {
        if (_callFinished)
        {
            _callFinished = false;
            Hide();
            _onCompleter();
            _onCompleter = null;
            Debug.LogFormat("窗口动画播放完成!");
        }
    }


    private void Hide()
    {
        //  Once 执行一次
        var anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.enabled = false;
        }
    }
}