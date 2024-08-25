using UnityEngine;
using System.Collections;
using System;
using Object = UnityEngine.Object;
/********************************
* Author：	    djl
* Date：       2016//
* Version：	V 0.1.0	
*******************************/
public class UnityObjectPools
{
    public Func<Object,bool> clearAction = null;
    static private UnityObjectPools instance = null;
    private ArrayList pools = new ArrayList();
	
    public static UnityObjectPools Instance()
    {
        if (instance == null)
            instance = new UnityObjectPools();
        return instance;
    }


    public void TryClearApp()
    {
        for (int i = 0; i < pools.Count; i++)
        {
            UnityObjectPool pool = pools[i] as UnityObjectPool;
            if (pool == null)
            {
                pools.RemoveAt(i);
                continue;
            }

            if (!pool.ifDestroyWhenApp)
            {
                continue;
            }

            if (pool.Using == null || pool.Using.Count == 0)
            {
                pool.Clear();
                pools.Remove(pool);
            }
            else
            {
                if (pool.IfClear())
                {
                    pool.ClearAvailable();
                }
            }
        }
    }


    public void TryClear()
    {
        for (int i = 0; i < pools.Count; i++)
        {
            UnityObjectPool pool = pools[i] as UnityObjectPool;
            if(pool ==null)
            {
                pools.RemoveAt(i);
                continue;
            }

            if(!pool.ifDestroyWhenApp)
            {
                continue;
            }

            if (Time.realtimeSinceStartup - pool.Last_UsingAll_Time > UnityObjectPool.UsingOffset)
            {
                if (pool.Using == null || pool.Using.Count == 0)
                {
                    pool.Clear();
                    pools.Remove(pool);
                }
                else
                {
                    if (pool.IfClear())
                    {
                        pool.ClearAvailable();
                    }
                }
            }         
        }
    }

    public UnityObjectPool Create(string name, Object obj,bool ifDestory = true)
    {
        UnityObjectPool pool = new UnityObjectPool(name, obj, ifDestory);
        pool.clearAction = clearAction;
        pools.Add(pool);
        return pool;
    }

    public void Remove(string name)
    {
        for (int i = 0; i < pools.Count; i++)
        {
            UnityObjectPool pool = pools[i] as UnityObjectPool;
            if (pool.name.Equals(name))
            {
                pool.Clear();
                pools.RemoveAt(i);
            }
        }
    }

    public void RemoveAll()
    {
        for (int i = 0; i < pools.Count; i++)
        {
            UnityObjectPool pool = pools[i] as UnityObjectPool;
           // if (pool.name == name)
            {
                pool.Clear();
               // pools.RemoveAt(i);
            }
        }
        pools.Clear();
        pools = null;
        instance = null;
    }

    public UnityObjectPool Get(string name)
    {
        for (int i = 0; i < pools.Count; i++)
        {
            UnityObjectPool mBreedPool = pools[i] as UnityObjectPool;
            if (mBreedPool == null)
            {
                pools.Remove(i);
                return null;
            }

            if (mBreedPool.name.Equals(name))
                return mBreedPool;
        }
        return null;
    }
}
