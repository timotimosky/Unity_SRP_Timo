using UnityEngine;
using System.Collections.Generic;

public class _CmdProcessor<T> where T : CmdOwner, new()
{
    private float generalHumanActionLifeTime = 0.3f;
    private int queueSize = 1;

    private T owner;

    public Dictionary<CmdType, CmdBaseBase> CmdDiction = new Dictionary<CmdType, CmdBaseBase>();

    private Queue<HumanActionRecord> waitCmdQueue = new Queue<HumanActionRecord>();

    public _CmdProcessor(T fighter)
    {
        owner = fighter;
    }
    public void CmdUpdate()
    {
        if (waitCmdQueue.Count > 0)
        {
            HumanActionRecord peekHumanActionRecord = waitCmdQueue.Peek();
            if (peekHumanActionRecord.isOverLifeTime(generalHumanActionLifeTime))
            {
                waitCmdQueue.Dequeue();
                return;
            }
            CmdType actionType = peekHumanActionRecord.actionType2;

            if (!CmdDiction.TryGetValue(actionType, out CmdBaseBase cmdBaseBase))
            {
                waitCmdQueue.Dequeue();
                return;
            }
            if (!cmdBaseBase.Condition())
            {
                return;
            }

            cmdBaseBase.Execute();
            waitCmdQueue.Dequeue();
        }
    }

    public CmdBaseBase AddCmd(CmdBaseBase mCmdBaseBase)
    {
        if (!CmdDiction.ContainsKey(mCmdBaseBase.cmdType))
            CmdDiction.Add(mCmdBaseBase.cmdType, mCmdBaseBase);
        return mCmdBaseBase;
    }

    public void DoInputAction(CmdType inputType)
    {
        if (!owner.AllowInput)
            return;
        //判断是否正在执行，不允许玩家重复执行
        DoCmd(inputType);
    }

    public void DoCmd(CmdType inputActionType)
    {
        DoCmd(inputActionType, Vector2.zero);
    }

    private bool IfCanReset(CmdBaseBase cmdBaseBase)
    {
        if ((!cmdBaseBase.canReset) && cmdBaseBase.isRuning)
            return false;
        return true;
    }


    public void DoCmd(CmdType inputActionType, Vector2 inputDirection)
    {
        if (!owner.initialized)
            return;

        if (!CmdDiction.TryGetValue(inputActionType, out CmdBaseBase cmdBaseBase))
        {
            return;
        }

        if (IfCanReset(cmdBaseBase) && cmdBaseBase.Condition())
        {
            cmdBaseBase.Execute();
            return;
        }

        HumanActionRecord newHumanActionRecord = new HumanActionRecord(inputActionType, inputDirection, Time.realtimeSinceStartup, generalHumanActionLifeTime);
        Debug.Log("存储操作 ");
        if (waitCmdQueue.Count >= queueSize)
        {
            waitCmdQueue.Dequeue();
            waitCmdQueue.Enqueue(newHumanActionRecord);
        }
        else
        {
            waitCmdQueue.Enqueue(newHumanActionRecord);
        }
    }

    public bool IsRuning(CmdType inputActionType)
    {

        if (!CmdDiction.TryGetValue(inputActionType, out CmdBaseBase cmdBaseBase))
        {
            return false;
        }
        return cmdBaseBase.isRuning;
    }

    public void Execute(CmdBaseBase cmdBaseBase)
    {
        cmdBaseBase.Execute();
    }


}
