using UnityEngine;
using System;
public class Singleton<T> where T : class, new()
{
    //
    // Static Fields
    //
    protected static T m_Instance;

    //
    // Static Properties
    //
    public static T Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new T();
            }
            return m_Instance;
        }
    }

    //
    // Static Methods
    //
    public static T GetInstance()
    {
        return Instance;
    }
}



public class SingletonMultithreading<T> where T : class
{
    private static T instance;
    private static readonly object InstanceLock = new object();

    public static T Instance
    {
        get
        {
            // 此处双重检测，确保多线程无冲突
            if (instance == null)
            {
                lock (InstanceLock)
                {
                    if (instance == null)
                    {
                        instance = (T)Activator.CreateInstance(typeof(T), true);
                    }
                }
            }
            return instance;
        }
    }
}