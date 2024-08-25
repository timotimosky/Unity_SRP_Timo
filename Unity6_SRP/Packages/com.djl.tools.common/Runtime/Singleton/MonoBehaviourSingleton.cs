using Djl.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoBehaviourSingleton<T> : CachedMonoBehaviour where T : MonoBehaviour
{
    protected static T m_instance;

    public static T Instance
    {
        get
        {
            if (null == m_instance)
            {
                m_instance = FindFirstObjectByType<T>();
                if (null == m_instance)
                {
                    GameObject obj = new GameObject();
                    m_instance = obj.AddComponent<T>();
                    obj.name = typeof(T).ToString();
                    obj.hideFlags = HideFlags.HideAndDontSave;
                }
            }
            return m_instance;
        }
    }

    public static void DestroyInstance()
    {
        if (m_instance == null)
            return;
       //GameObject gameObj = m_instance.CachedGameObject;
        // ResourceMgr.Instance.DestroyObject(gameObj);
    }
}

