using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CmdOwner : MonoBehaviour
{

    public bool initialized;
    public bool AllowInput =true;


    public virtual void DoCmd(CmdType inputType)
    {
    }
}
