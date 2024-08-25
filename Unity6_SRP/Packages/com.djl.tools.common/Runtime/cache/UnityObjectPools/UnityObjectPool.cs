using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
public class UnityObjectPool
{
    public Func<Object, bool> clearAction = null;
    public string name = "";
    public Object baseObj = null;
    private Stack available;  //未使用
    public ArrayList Using;   //使用 todo使用未装箱

    public bool ifDestroyWhenApp;

    public float Last_UsingAll_Time;

    public const int UsingOffset = 60;

    public bool IfClear()
    {
        if (available == null || available.Count <= 0)
        {
            return false;
        }

        if (Time.realtimeSinceStartup - Last_UsingAll_Time > UsingOffset)
        {
            return true;
        }
        return false;
    }

    public void ClearAvailable()
    {
        Object go;
        int leg = available.Count;
        for (int i = 0; i < leg; i++)
        {
            go = available.Pop() as Object;
            Object.Destroy(go);
        }
        available.Clear();
        go = null;
    }


    public UnityObjectPool(string name, Object obj, bool ifDestory = true)
    {
        ifDestroyWhenApp = ifDestory;
        this.name = name;
        this.baseObj = obj;
        available = new Stack();
        Using = new ArrayList();
    }


    private void TryReset()
    {
        Last_UsingAll_Time = Time.realtimeSinceStartup;
    }

    public GameObject Spawn()
    {
        if (baseObj == null)
        {
            return null;
        }

        GameObject go;

        if (available.Count == 0)
        {
            go = Object.Instantiate(baseObj) as GameObject;
            if (go == null)
            {
                return null;
            }
            TryReset();
        }
        else
        {
            go = available.Pop() as GameObject;
            if (go == null)
            {
                return Spawn();
            }
            if (available.Count == 0)
            {
                TryReset();
            }
        }
        if (available.Contains(go))
        {
            Debug.LogFormat("异常---------------------对象池");
        }

        Using.Add(go);
        go.name = name;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.SetActive(true);
        return go;
    }

    public void Unspawn(GameObject go, bool accident = false)
    {
        Using.Remove(go);
        if (accident)
        {
            GameObject.Destroy(go);
            return;
        }

        if (IfClear())
        {
            GameObject.Destroy(go);
            return;
        }

        if (!available.Contains(go))
        {
            available.Push(go);
            go.SetActive(false);
        }
        else
        {
            go.SetActive(false);
           // DebugTool.LogFormat((object)"回收失败", go.name);
        }
    }

    public void UnspawnAll()
    {
        GameObject go;
        for (int i = 0; i < Using.Count; i++)
        {
            go = Using[i] as GameObject;
            if (go != null && go.activeSelf)
                Unspawn(go);
        }
    }

    public void Clear()
    {
        int oldLegth = available.Count;
        for (int i = 0; i < oldLegth; i++)
        {
            Object go = available.Pop() as Object;
            Object.Destroy(go);
        }
        available.Clear();
        available = null;

        oldLegth = Using.Count;
        for (int i = 0; i < oldLegth; i++)
        {
            Object go = Using[i] as Object;
            Object.Destroy(go);
        }
        Using.Clear();
        Using = null;
        if(clearAction!=null)
            clearAction(baseObj);

        baseObj = null;
    }


}
