using System;
using UnityEngine;

public sealed class TimerInfo
{
    public bool IsFinish;
    public bool IsLoop;
    public bool IsReal;
    public float PassTime;
    public float StartTime;
    public float IntervalTime;
    public object data;
    public Action<object> onCall;

    public TimerInfo()
    {
        Reset();
    }

    public void Reset()
    {
        PassTime = 0;
        IsFinish = false;
        StartTime = Time.realtimeSinceStartup;
        onCall = null;
    }


    public bool Tick()
    {
        if (IsLoop)
        {
            PassTime += IsReal ? Time.unscaledDeltaTime : Time.deltaTime;
            if (PassTime >= IntervalTime)
            {
                PassTime = 0f;
                return true;
            }
            return false;
        }

        else
        {
            if (IsReal)
                return Time.unscaledDeltaTime - StartTime >= IntervalTime;

            PassTime += Time.deltaTime;
            if (PassTime >= IntervalTime)
                IsFinish = true;

            return IsFinish;
        }

    }


    public void Call()
    {
        if (onCall != null)
            onCall(data);
    }

}