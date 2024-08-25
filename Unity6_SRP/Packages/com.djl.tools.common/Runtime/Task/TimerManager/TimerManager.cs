using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class TimerManager : Singleton<TimerManager>
{

    private static GenericPool<TimerInfo> pool = new GenericPool<TimerInfo>(CreateTimerInfo, ResetTimerInfo);

    private static TimerInfo CreateTimerInfo()
    {
        return new TimerInfo();
    }

    private static  void ResetTimerInfo(TimerInfo outTimerInfo)
    {
        outTimerInfo.Reset();
    }

    List<TimerInfo> mTimerList = new List<TimerInfo>();

    static public TimerInfo Invoke ( float timer , Action<object> call , object data = null , bool isreal = false, bool isloop = false)
    {
        TimerInfo info = pool.New();

        info.IsReal = isreal;
        info.IsLoop = false;
        info.IntervalTime = timer;
        info.onCall = call;
        info.data = data;
        info.IsLoop = isloop;

        Instance.mTimerList.Add(info);

        return info;
    }


    static public void Remove(TimerInfo removeInfo)
    {
        Instance.mTimerList.Remove(removeInfo);
        pool.Store(removeInfo);
    }


    private TimerInfo info_cache;
    public void DoUpdate()
    {
        for (int i = mTimerList.Count - 1; i >= 0; i--) //逆序遍历，保证删除的正确性
        {
            info_cache = mTimerList[i];

            if (info_cache.Tick())
            {
                info_cache.Call();
                if (info_cache.IsFinish)
                {
                    Remove(info_cache);
                    //mTimerList.RemoveAt(i);
                }
            }

        }
    }


}
