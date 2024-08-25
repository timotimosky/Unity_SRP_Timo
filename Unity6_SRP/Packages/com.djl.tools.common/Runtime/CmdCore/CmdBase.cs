using UnityEngine;

/********************************
 * Author   :    DJL
 * Date     :    2015年8月2日
 * Version  :    V 0.1.0
 * Describe :    这是技能超类，并且与动画系统公用枚举
 * ******************************/

public abstract class CmdBaseBase
{
    //类型
    public CmdType cmdType { get; protected set; }

    //判断是否正在执行，不允许玩家重复执行
    public bool canReset = false;
    public bool isRuning = false;
    public abstract void Execute();
    public virtual bool Condition()
    {
        if ((!canReset) && isRuning)
        {
            Debug.Log("重复命令 ");
            return false;
        }
        return true;
    }
    public virtual bool Break()
    {
        isRuning = false;
        return true;
    }


}

//所有指令，通过这里控制：移动、攻击、技能
public abstract class CmdBase<T> : CmdBaseBase where T : MonoBehaviour, new()
{
    public AnimatorStateInfo AnimatorStateInfo;

    public T humamController { get; private set; }


    public CmdBase(T obj, CmdType type)
    {
        humamController = obj;
        cmdType = type;
    }
}
