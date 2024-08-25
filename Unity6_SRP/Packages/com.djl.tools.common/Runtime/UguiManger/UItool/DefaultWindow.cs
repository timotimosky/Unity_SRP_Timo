using UnityEngine;
using System.Collections;
//using LuaInterface;

/********************************
 * Author		：	cc
 * Date			：	2016/8/9
 * Version		：	V 0.1.0	
 * lua 专用
 * 用于ui方法转发到lua
 *******************************/


public class DefaultWindow : UguiBase
{

    //private LuaBehaviour _luaBehaviour;

    //private LuaBehaviour LuaAgent
    //{
    //    get
    //    {
    //        if (_luaBehaviour == null)
    //            _luaBehaviour = GetComponent<LuaBehaviour>();
    //        return _luaBehaviour;
    //    }
    //}



   // protected override void RegisterBinding()
   // {
   // //    base.RegisterBinding();
   ////     LuaAgent.CallFunction("RegisterBinding");
   // }


    public void OnLuaDestory()
    {
      //  LuaAgent.CallFunction("RemoveBinding");
    }

    public override void ExecuteParmetersBeforeOpen(params object[] args)
    {
        base.ExecuteParmetersBeforeOpen(args);
      //  LuaAgent.CallFunction("ExecuteParmetersBeforeOpen", args);
    }
}
