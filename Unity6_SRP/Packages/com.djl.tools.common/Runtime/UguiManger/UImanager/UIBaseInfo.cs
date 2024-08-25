using UnityEngine;
using System.Collections;

/// <summary>
/// 用于层级显示
/// </summary>
public enum UILayer
{
    BaseLayer, //最底层
    LowLayer = 1, //战斗UI等,飘血等，略低于普通UI
    Normal, //普通UI层
    Middle, //中间层, 显示在UI上层的一些UI特效
    Tips, //Tips层 长按显示
    Rewards,//奖励层
    TopUILabyer, //最上层,弹出消息框等,互斥
    Guild,  //指引层
    Wait,
    Download,   //下载层
    //NoneLayer, //没有层级，跟随父物体层级
    //STORYLayer,     //剧情界面，剧情界面出现，所有UI关闭.剧情界面关闭时，其他UI恢复
    TopTopTop,
    TopTopTopTop,
}

/********************************
 * Author   :    DJL
 * Date     :    2017年8月22日
 * Version  :    V 0.1.0
 * Describe :    UI监视板信息
 * ******************************/
[ExecuteInEditMode]
[System.Serializable]
public class UIBaseInfo : MonoBehaviour 
{
    [HideInInspector][SerializeField] public string mResPath; //资源相对路径
    [HideInInspector][SerializeField] public UguiBase mScript;
    [HideInInspector][SerializeField] public System.Type mOwnerType;
    public UILayer Layer;
    public UseType mUseType = UseType.Scene;
    // 是否为全屏UI,会自动生成遮罩 TODO：以后不用一张新的遮罩，直接把最上层的UI碰撞体拉伸，就点击不到下层物体
    public bool IsFullUI = false;
    public bool Ani;// 彈窗動畫
}
