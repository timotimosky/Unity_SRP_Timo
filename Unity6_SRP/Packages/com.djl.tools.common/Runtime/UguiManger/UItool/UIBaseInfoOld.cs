/********************************
* Author：	    djl
* Date：       2016//
* Version：	V 0.1.0	
* * 设置类型
*******************************/

using System.Collections.Generic;



public class UIBaseInfoOld : Config<UIBaseInfoOld>, ConfigInterface
{
    /// <summary>
    /// string
    /// </summary>
    public string id;

    /// <summary>
    /// 资源相对路径
    /// </summary>
    public string Path;

    /// <summary>
    /// 窗口层次
    /// </summary>
    public UILayer Layer;


    /// <summary>
    /// 实例生存周期 int
    /// </summary>
    public int UseTypeInt;

    /// <summary>
    /// 实例生存周期
    /// </summary>
    public UseType mUseType = UseType.Once;

    /// <summary>
    /// 是否为全屏UI,会自动生成遮罩 TODO：这里以后不用一张新的遮罩，直接把最上层的UI 碰撞体拉伸，就点击不到下层物体了
    /// </summary>
    public bool IsFullUI;

    /// <summary>
    /// 彈窗動畫
    /// </summary>
    public bool Ani;

    /// <summary>
    /// 跳转场景不销毁
    /// </summary>
    public bool DontDestoryOnJumpScene;


    //public override List<UguiDescription> contentList
    //{
    //    get
    //    {
    //        if (data != null)
    //        {

    //            var list = NewtonsoftMoe.Json.JsonConvert.DeserializeObject<List<UguiDescription>>(System.Text.Encoding.UTF8.GetString(data));
    //            _contentList.AddRange(list);

    //            data = null;
    //        }
    //        return _contentList;
    //    }
    //}


    public void AddFirstToWindows(List<UIBaseInfoOld> addList )
    {
        if (contentList == null)
        {
            _contentList = new List<UIBaseInfoOld>();   
        }

        _contentList.AddRange(addList);
    }


    public override string ConfigPath()
    {
        return "Firstload/WindowSetting";
    }

    public override string GetStringId()
    {
        return this.id;
    }

}