using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanActionRecord
{
    public float triggeredTime;
    public CmdType actionType2;
    public Vector2 direction;
    public float lifeTime;//every Action can have its own liftTime

    public HumanActionRecord(CmdType inputActionType, Vector2 inputDirection, float inputTriggeredTime, float inputLifeTime = -1)
    {
        triggeredTime = inputTriggeredTime;
        actionType2 = inputActionType;
        direction = inputDirection;
        lifeTime = inputLifeTime;
    }


    public bool isOverLifeTime(float generalLifeTimeLimit)
    {
        float deltaTime = Time.realtimeSinceStartup - triggeredTime;
        if ((deltaTime > lifeTime) && (lifeTime > 0))
            return true;
        if (deltaTime > generalLifeTimeLimit)
            return true;
        return false;
    }




}

